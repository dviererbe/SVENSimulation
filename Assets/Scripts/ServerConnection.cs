
using System;
using System.IO;
using System.Net;
using UnityEngine;

public class ServerConnection 
{
    //needed for test at home
    private String user = "tsuksdl";   //Usrename für die Anmeldung
    private String password = "A00Schottisch!"; //Password for the usere on the FHEM server
    private bool authentifiction = true;    //server requiers an authemntifikation


    private static ServerConnection instance = null;
    private String csrfToken = null;    //token für die Authentifikation
    private String serverip = "192.168.1.102:8083";   //IP des FHEMservers

    private ServerConnection()
    {
        getcsrfToken();
    }

    public static ServerConnection getInstance()
    {
        if(instance == null)
        {
            instance = new ServerConnection();
        }
        return instance;
    }

    public Boolean setData(String device, String value)
    {
        if (csrfToken.Equals(null))
        {
            if (!getcsrfToken())
            {
                return false;
            }
        }

        String url = "http://" + serverip + "/fhem?cmd=set%20" + device + "%20" + value +
             "&fwcsrf=" + csrfToken + 
             "&XHR=1";
        Debug.Log(url);
        Uri uri = new Uri(url);

        WebRequest request = WebRequest.CreateHttp(uri);

        if (authentifiction)
        {
            //Anmeldung bei dem Server
            NetworkCredential myNetworkCredential = new NetworkCredential(user, password);

            CredentialCache myCredentialCache = new CredentialCache();
            myCredentialCache.Add(uri, "Basic", myNetworkCredential);

            request.PreAuthenticate = true;
            request.Credentials = myCredentialCache;
        }
        else
        {
            request.Credentials = CredentialCache.DefaultCredentials;
        }

        try
        {
            WebResponse response = request.GetResponse();
            if(!((HttpWebResponse)response).StatusDescription.Equals("OK"))
            {
                return false;
            }
            response.Close();
        }
        catch(WebException e)
        {
            Debug.Log(e.ToString());
            csrfToken = null;
            return false;
        }

        return true;
    }

    public String getData(String device, String attribut)
    {
        if (csrfToken.Equals(null))
        {
            if (!getcsrfToken())
            {
                return null;
            }
        }
        String value = null;
        String url = "http://" + serverip + "/fhem?cmd=get%20" + device + "%20" + attribut +
             "&fwcsrf=" + csrfToken +
             "&XHR=1";
        Debug.Log(url);
        Uri uri = new Uri(url);

        WebRequest request = WebRequest.CreateHttp(uri);

        if (authentifiction)
        {
            //Anmeldung bei dem Server
            NetworkCredential myNetworkCredential = new NetworkCredential(user, password);

            CredentialCache myCredentialCache = new CredentialCache();
            myCredentialCache.Add(uri, "Basic", myNetworkCredential);

            request.PreAuthenticate = true;
            request.Credentials = myCredentialCache;
        }
        else
        {
            request.Credentials = CredentialCache.DefaultCredentials;
        }

        try
        {
            WebResponse response = request.GetResponse();
            using (Stream datastream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(datastream);
                value = reader.ReadToEnd();
                reader.Close();
                datastream.Close();
            }
            response.Close();
        }
        catch (WebException e)
        {
            Debug.Log(e.ToString());
            csrfToken = null;
            return null;
        }

        return value;
    }

    private bool getcsrfToken()
    {
        String url = "http://" + serverip + "/fhem";
        Debug.Log(url);
        Uri uri = new Uri(url);

        WebRequest request = WebRequest.CreateHttp(uri);

        if (authentifiction)
        {
            //Anmeldung bei dem Server
            NetworkCredential myNetworkCredential = new NetworkCredential(user, password);

            CredentialCache myCredentialCache = new CredentialCache();
            myCredentialCache.Add(uri, "Basic", myNetworkCredential);

            request.PreAuthenticate = true;
            request.Credentials = myCredentialCache;
        }else
        {
            request.Credentials = CredentialCache.DefaultCredentials;
        }

        try
        {
            WebResponse response = request.GetResponse();

            using (Stream datastream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(datastream);
                while(!reader.EndOfStream)
                {
                    String responsFromServer = reader.ReadLine();
                    String[] tags = responsFromServer.Split(new char[] { '<', '>', ' ', '"' });
                    for(int i = 0; i < tags.Length; i++)
                    {
                        if(tags[i].Equals("fwcsrf"))
                        {
                            csrfToken = tags[i + 3];
                            goto Found;
                        }
                    }
                }
            Found:
                reader.Close();
                datastream.Close();
            }

            response.Close();
        }
        catch(WebException e)
        {
            csrfToken = null;
            return false;
        }
        Debug.Log(csrfToken);
        return true;
    }
}
