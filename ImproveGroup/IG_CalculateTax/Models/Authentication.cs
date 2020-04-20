using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Crm.Sdk.Messages;
using System.Net;
using Microsoft.Xrm.Sdk.Query;
using System.Configuration;
using System.ServiceModel.Description;

namespace IG_ImproveGroup_Web_API.Models
{
    public class Authentication
    {
        public static IOrganizationService OrgService()
        {
            try
            {
                string orgURL = ConfigurationManager.AppSettings["OrgURL"].ToString();
                string userName = ConfigurationManager.AppSettings["userName"].ToString();
                string password = ConfigurationManager.AppSettings["password"].ToString();
                ClientCredentials clientCredentials = new ClientCredentials();
                clientCredentials.UserName.UserName = userName;
                clientCredentials.UserName.Password = password;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                IOrganizationService service = (IOrganizationService)new OrganizationServiceProxy(new Uri(orgURL),
                 null, clientCredentials, null);
                return service;
            }
            catch
            {
                return null;
            }


        }
    }
}