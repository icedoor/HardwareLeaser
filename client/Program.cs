using System;
using RestSharp;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace client
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1) {
                PrintMenu();
                return;
            }

            string dockerHost = Environment.GetEnvironmentVariable("DB_HOST");
            var client = new RestClient(dockerHost == null ? "http://localhost:5000" : dockerHost);
         
            Console.WriteLine();

            if (string.Equals(args[0], "machines")) {
                string res = GetMachines(client, args);
                PrintMachines(res);
            } else if (string.Equals(args[0], "add")) {
                string res = AddMachine(client, args);
                Console.WriteLine(res);
            } else if (string.Equals(args[0], "lease")) {
                string res = LeaseMachine(client, args);
                Console.WriteLine(res);
            } else if (string.Equals(args[0], "leases")) {
                string res = GetLeases(client);
                PrintLeases(res);
            } else if (string.Equals(args[0], "help")) {
                PrintMenu();
            }
            else {
                Console.WriteLine("Input not valid, try again.");   
            }
        }

        static void PrintMenu() {
            Console.Clear();
            Console.WriteLine("Welcome to this hardware leasing application!\n");
            Console.WriteLine("machines                    Get all machines.");
            Console.WriteLine("machines <platform>         Get all machines with platform.");
            Console.WriteLine("add <ip> <name> <platform>  Add a machine with ip, name and platform. Platform should be PC, PS4 or XboxOne.");
            Console.WriteLine("lease <platform> <minutes>  Lease a machine of platform type for a number of minutes.");
            Console.WriteLine("leases                      Get all active leases.");
            Console.WriteLine("help                        Print this help menu.");
        }

        static void PrintMachines(string data) {
            try {
                Machines jsonMachines = JsonConvert.DeserializeObject<Machines>(data);
                if (jsonMachines.machines.Count < 1) {
                    throw new Exception("No machines found.");
                }
                Console.WriteLine(String.Format("|--------------------MACHINES--------------------|"));
                Console.WriteLine(String.Format("|{0,15}|{1,10}|{2,10}|{3,10}|", "IP", "Name", "Platform", "Active"));
                Console.WriteLine(String.Format("|------------------------------------------------|"));
                foreach(Machine m in jsonMachines.machines) {
                    Console.WriteLine(String.Format("|{0,15}|{1,10}|{2,10}|{3,10}|", m.ip, m.name, m.platform, m.isActive));
                }
            } catch (Exception ex) {
                Console.WriteLine("No machines found. You can add new machines with the 'add' keyword. See 'help' for details.");
            }
        }

        static void PrintLeases(string data) {
            try {
                Leases jsonLeases = JsonConvert.DeserializeObject<Leases>(data);
                if (jsonLeases.leases.Count < 1) {
                    throw new Exception("No active leases found.");
                }
                Console.WriteLine(String.Format("|-------------------ACTIVE LEASES---------------------|"));
                Console.WriteLine(String.Format("|{0,15}|{1,10}|{2,10}|{3,15}|", "IP", "Name", "Platform", "Minutes left"));
                Console.WriteLine(String.Format("|-----------------------------------------------------|"));
                foreach(Lease l in jsonLeases.leases) {
                    Console.WriteLine(String.Format("|{0,15}|{1,10}|{2,10}|{3,15}|", l.ip, l.name, l.platform, l.minLeft));
                }  
            } catch (Exception ex) {
                Console.WriteLine("No active leases found. You can lease a machines with the 'lease' keyword. See 'help' for details.");
            }
        }

        static bool CheckParams(string[] parameters) {
            foreach (string p in parameters) {
                if (String.IsNullOrWhiteSpace(p)) {
                    return false;
                }
            }
            return true;
        }

        static string GetMachines(RestClient client, string[] parameters) {
            if (parameters.Length == 1) {
                return GetMachines(client);
            }

            if (parameters.Length != 2 || !CheckParams(parameters)) {
                return "Platform parameter must be provided.";
            }
            var request = new RestRequest("machines/{platform}", Method.GET);
            request.AddUrlSegment("platform", parameters[1]);
            IRestResponse response = client.Execute(request);

            return response.Content;
        }

        static string GetMachines(RestClient client) {
            var request = new RestRequest("machines", Method.GET);
            IRestResponse response = client.Execute(request);

            return response.Content;
        }

        static string GetLeases(RestClient client) {
            var request = new RestRequest("leases", Method.GET);
            IRestResponse response = client.Execute(request);

            return response.Content;
        }

        static string AddMachine(RestClient client, string[] parameters) {
            if (parameters.Length != 4 || !CheckParams(parameters)) {
                return "ip, name and platform parameter must be provided.";
            }

            var request = new RestRequest("addmachine", Method.POST);
            request.AddJsonBody(new { ip = parameters[1], name = parameters[2], platform = parameters[3] });
            request.AddHeader("Content-Type", "application/json");
            IRestResponse response = client.Execute(request);

            if (String.IsNullOrWhiteSpace(response.Content)) {
                return "Machine could not be added.";
            }

            return response.Content;
        }

        static string LeaseMachine(RestClient client, string[] parameters) {
            if (parameters.Length != 3 || !CheckParams(parameters)) {
                return "Platform and minute parameter must be provided.";
            }

            var request = new RestRequest("lease", Method.POST);
            request.AddJsonBody(new { platform = parameters[1], minutes = parameters[2] });
            request.AddHeader("Content-Type", "application/json");
            IRestResponse response = client.Execute(request);
            
            if (String.IsNullOrWhiteSpace(response.Content)) {
                return "Machine could not be leased.";
            }

            return response.Content;
        }
    }

    class Machines
    {
        public List<Machine> machines { get; set; }
    }

    class Machine
    {
        public string ip { get; set; }
        public string name { get; set; }
        public string platform { get; set; }
        public bool isActive { get; set; }
    }

    class Leases
    {
        public List<Lease> leases { get; set; }
    }

    class Lease
    {
        public string ip { get; set; }
        public string name { get; set; }
        public string platform { get; set; }
        public float minLeft { get; set; }
    }
}
