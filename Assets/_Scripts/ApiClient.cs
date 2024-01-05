using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ApiClient
{
    private string baseUrl;
    private HttpClient httpClient;

    public ApiClient(string baseUrl)
    {
        this.baseUrl = baseUrl;
        httpClient = new HttpClient();
    }

    public async Task<string> Get(string endpoint)
    {
        string responseContent = null;
        
        HttpResponseMessage response = await httpClient.GetAsync(new Uri(baseUrl + endpoint));
        if (response.IsSuccessStatusCode)
        { 
            responseContent = await response.Content.ReadAsStringAsync();
        }
        else
        {
            // Manejar errores aquí
            Debug.LogError("Error");
        }


        return responseContent;
    }
    
    public async Task<string> Post(string endpoint, string jsonData)
    {
        string responseContent = null;
        
        //If jsondata doesnt have " or { at the start and end, add them
        if (!jsonData.StartsWith("{") && !jsonData.StartsWith("\""))
        {
            jsonData = "\"" + jsonData;
        }

        if (!jsonData.EndsWith("}") && !jsonData.EndsWith("\""))
        {
            jsonData += "\"";
        }
        
        string json = "{";
        json +=  "\"data\":" + jsonData;
        json += "}";
            
        //Debug.Log(json);
            
        StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await httpClient.PostAsync(new Uri(baseUrl + endpoint), content);

        if (response.IsSuccessStatusCode)
        {
            responseContent = await response.Content.ReadAsStringAsync();
        }
        else
        {
            Debug.LogError(response.StatusCode);
        }
        

        return responseContent;
    }

}