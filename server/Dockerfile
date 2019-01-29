FROM python:3.6-alpine

WORKDIR /app

COPY . /app

RUN pip install -r requirements.txt

# Make port 80 available outside this container
EXPOSE 80

CMD [ "python", "app.py" ]