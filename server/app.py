from flask import Flask, request, g, jsonify
import json
import sqlite3
import time

app = Flask(__name__)

# The path to our database.
DATABASE = 'database.db'

@app.route('/')
def api():
    return '''/addmachine {ip,name,platform} | /lease {platform,minutes}
    | /machines | /machines/[platform] | /leases'''

def get_db():
    '''Gets the database connection

    :return: an open database connection
    '''
    db = getattr(g, '_database', None)
    if db is None:
        db = g._database = sqlite3.connect(DATABASE)
    return db

@app.teardown_appcontext
def close_connection(exception):
    '''Closes the database connection
    '''
    db = getattr(g, '_database', None)
    if db is not None:
        db.close()

def query_fetch(query, args=()):
    '''Fetches from the database.

    :param query: the sql query to run
    :param args: the arguments to run query with, may be empty
    :return: all fetched results of the query
    '''
    cur = get_db().execute(query, args)
    rv = cur.fetchall()
    cur.close()
    return rv

def query_commit(query, args=()):
    '''Commits new data to the database.

    :param query: the sql query to run
    :param args: the arguments to run query with, may be empty
    :return: true if change was made, false if not
    '''
    db = get_db()
    cur = db.execute(query, args)
    db.commit()
    cur.close()
    return cur.rowcount > 0

@app.route('/addmachine', methods=['POST'])
def add_machine():
    '''Add a machine to the database.
    Machine ip, name and platform bust be supplied in request body.
    curl http://localhost:5000/addmachine
    -H 'Content-Type: application/json' POST 
    -d '{\'ip\':\'192.168.1.6\',\'name\':\'HW6\',\'platform\':\'PS4\'}'

    :return: status message
    '''

    try:
        data = request.get_json()
        platform = data.get('platform')
        if (platform != "PC" and platform != "PS4" and platform != "XboxOne"):
            return "Platform must be PC, PS4 or XboxOne"

        machine = (data.get('ip'), data.get('name'), platform,)
        sql = ''' INSERT INTO machines(ip,name,platform) VALUES(?,?,?) '''

        success = query_commit(sql, machine)

        if (success) :
            return "Machine added!", 200

        return "Machine could not be added."
    except Exception as e:
        return "Machine could not be added. Error:" + str(e)


@app.route('/lease', methods=['POST'])
def lease_machine():
    '''Leases a machine.
    Machine platform and time in minutes to lease bust be supplied in
    request body.
    curl http://localhost:5000/lease
    -H 'Content-Type: application/json' -X POST
    -d '{\'platform\':\'XboxOne\',\'minutes\':\'5\'}'

    :return: status message
    '''
    try:
        data = request.get_json()
        platform = data.get('platform')
        minutes = int(data.get('minutes'))

        now = int(time.time())
        leaseTo = now + (minutes * 60)

        sql = '''UPDATE machines SET leasedTo = ? WHERE ip =
        (SELECT ip from machines WHERE platform = ? AND
        (leasedTo IS NULL OR leasedTo < ?) LIMIT 1)'''

        success = query_commit(sql, (leaseTo, platform, now))

        if (success) :
            return 'Lease successful!'

        return 'Machine could not be leased.'
    except Exception as e:
        return "Machine could not be leased. Error:" + str(e)

def format_machines_response(machines):
    '''Creates a response for all machines.

    :param machines: the machines to format
    :return: a JSON with all formatted machine objects
    '''
    now = int(time.time())
    response = []
    for m in machines:
        leasedTo = m[3]
        isActive = (leasedTo > now) if leasedTo else False
        formatted = {
            'ip' : m[0],
            'name' : m[1],
            'platform' : m[2],
            'isActive' : isActive
        }
        response.append(formatted)

    return json.dumps({'machines' : list(response)})

@app.route('/machines')
def get_machines():
    '''Gets all machines.
    curl http://localhost:5000/machines

    :return: a JSON of all machines
    '''
    sql = '''SELECT * from machines'''
    machines = query_fetch(sql)
    
    return format_machines_response(machines)

@app.route('/machines/<platform>')
def get_machines_by_platform(platform):
    '''Gets all machines of a specified platform.
    curl http://localhost:5000/machines/XboxOne

    :param platform: the platform of the machines to get
    :return: a JSON of all machines
    '''
    sql = '''SELECT * from machines WHERE platform = ?'''
    machines = query_fetch(sql, (platform,))

    return format_machines_response(machines)

@app.route('/leases')
def get_leases():
    '''Gets all active leases.
    curl http://localhost:5000/leases

    :return: a JSON of all active leases
    '''
    now = int(time.time())
    sql = '''SELECT * from machines WHERE leasedTo > (?)'''
    leases = query_fetch(sql, (now,))

    now = int(time.time())

    response = []
    for l in leases:
        leasedTo = l[3]
        minLeft = round((leasedTo - now) / 60, 2)
        formatted = {
            'ip' : l[0],
            'name' : l[1],
            'platform' : l[2],
            'minLeft' : minLeft
        }
        response.append(formatted)

    return json.dumps({'leases' : list(response)})

if __name__ == "__main__":
    app.run(host='0.0.0.0', port=80)