using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static string TID = "483146";
        static string User_name = "int_assessment";
        static string PWD = "--user your own password--";
        static string CID = "rwarDFGiyVDFfm9PLoJSBIIC2l6zinJGc93Nasq7";
        static string Client_secret = "--user your own client_secret--";
        static string Grant_type = "password";
        static string access_token = "";
        static string Seq = DateTime.Now.ToString("MMdd-HHmmss");

        static void Main(string[] args)
        {
            try
            {
                //Joor Sandbox Web Portal: https://sandbox.jooraccess.com/
                
                initial();

                String sid = "3619587";
                sid = AddProduct("tugamobi SA01", "TUGA-", "TUGASA01-", "This product is for testing", 1);

                String[] colors2 = { "White"}; 
                String[] sizes2 = { "Large"};
                AddSKUToProduct(sid, colors2, sizes2, "MPN-" + sid);

                PrintProducts();

                PrintProductSKUs(sid);

                Console.Read();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }


        }
        static void initial()
        {
            var client = new RestClient("https://atlas-sandbox.jooraccess.com/auth");
            var request = new RestRequest(Method.POST);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("content-type", "application/x-www-form-urlencoded; boundary=---011000010111000001101001");
            request.AddParameter("multipart/form-data; boundary=---011000010111000001101001", "client_id=" + CID + "&grant_type=" + Grant_type + "&client_secret=" + Client_secret + "&username=" + User_name + "&password=" + PWD, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            dynamic results = JsonConvert.DeserializeObject<dynamic>(response.Content);
            access_token = results.access_token;
            Console.WriteLine("Got Token:");
            Console.WriteLine(access_token);
            Console.WriteLine("---------------------------------------------------");
            Debug.WriteLine(access_token);
        }
        static void PrintCategory()
        {
            var client = new RestClient("https://apisandbox.jooraccess.com/v4/categories?account=" + TID);
            var request = new RestRequest(Method.GET);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Authorization", access_token);
            IRestResponse response = client.Execute(request);
            Debug.WriteLine(response.Content);
        }

        static void PrintProducts()
        {
            var client = new RestClient("https://apisandbox.jooraccess.com/v4/products?account=" + TID);
            var request = new RestRequest(Method.GET);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Authorization", access_token);
            IRestResponse response = client.Execute(request);
            dynamic results = JsonConvert.DeserializeObject<dynamic>(response.Content);
            ArrayList rt = new ArrayList();
            Console.WriteLine("The TUGA product list");
            foreach (var obj in results.data)
            {
                rt.Add(obj);
                if (obj.external_id.ToString().StartsWith("TUGA"))
                {                    
                    Console.WriteLine("#####");
                    Console.WriteLine(obj.id);
                    Console.WriteLine(obj.name);
                    Console.WriteLine(obj.external_id);
                    Console.WriteLine(obj.product_identifier);
                    Console.WriteLine("#####");
                }
            }
            Console.WriteLine("---------------------------------------------------");
            Debug.WriteLine(JsonConvert.SerializeObject(rt));
        }

        static void PrintProductSKUs(String sid)
        {
            var client = new RestClient("https://apisandbox.jooraccess.com/v4/skus?account=" + TID + "&product_ids=" + sid);
            var request = new RestRequest(Method.GET);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Authorization", access_token);
            IRestResponse response = client.Execute(request);

            dynamic results = JsonConvert.DeserializeObject<dynamic>(response.Content);
            Console.WriteLine("The SKU list: " + results.data[0].id);
            foreach (var obj in results.data[0].trait_values)
            {
                    Console.WriteLine("#####");
                    Console.WriteLine(obj.value);
                    Console.WriteLine("#####");
            }
            Console.WriteLine("---------------------------------------------------");
            Debug.WriteLine(response.Content);
        }
        static String AddProduct(String name, String eid, String pid, String desc, int minqty)
        {
            var client = new RestClient("https://apisandbox.jooraccess.com/v4/products/bulk_create?account=" + TID);
            var request = new RestRequest(Method.POST);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Authorization", access_token);
            request.RequestFormat = DataFormat.Json;
            request.AddParameter("application/json", "[{\"name\":\"" + name + "\",\"external_id\":\""+ eid + Seq + "\",\"product_identifier\":\"" + pid + Seq + "\",\"description\":\"" + desc + "\",\"order_minimum\":" + minqty + "}]", ParameterType.RequestBody);

            var response = client.Execute(request);
            dynamic results = JsonConvert.DeserializeObject<dynamic>(response.Content);
            Console.WriteLine("Successfully add a new product:" + name +", "+ results.data[0].id);
            Console.WriteLine("---------------------------------------------------");
            Debug.WriteLine(response.Content);
            return results.data[0].id;
        }

        static void AddSKUToProduct(String sid, String[] colors, String[] sizes, String mpn)
        {
            String template = "[{\"product_id\":\"" + sid + "\",\"trait_values\":[#TRAITS#],\"identifiers\":[#IDENTIFIERS#]}]";
            String trait_color_template = "{\"trait_name\":\"Color\",\"value\":\"#COLOR#\",\"external_id\":\"#COLOR#\"},";
            String trait_size_template = "{\"trait_name\":\"Size\",\"value\":\"#SIZE#\",\"external_id\":\"#SIZE#\"},";
            String identifier_template = "{\"type\":\"upc\",\"value\":\"#MPN#\"}";

            String trait_colors = "", trait_sizes = "", identifier = "";
            foreach (var val in colors)
            {
                trait_colors += trait_color_template.Replace("#COLOR#", val);
            }
            foreach (var val in sizes)
            {
                trait_sizes += trait_size_template.Replace("#SIZE#", val);
            }
            identifier = identifier_template.Replace("#MPN#", mpn);
            template = template.Replace("#TRAITS#",trait_colors + trait_sizes.Substring(0,trait_sizes.Length-1)).Replace("#IDENTIFIERS#", identifier);

            var client = new RestClient("https://apisandbox.jooraccess.com/v4/skus/bulk_create?account=" + TID);
            var request = new RestRequest(Method.POST);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Authorization", access_token);
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", template, ParameterType.RequestBody);
            var response = client.Execute(request);

            Console.WriteLine("Successfully add SKUs to a product:" + sid);
            Console.WriteLine("---------------------------------------------------");
            Debug.WriteLine(response.Content);
        }
    }
}
