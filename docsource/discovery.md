When scheduling discovery jobs in Keyfactor Command, there are a few fields that are important to highlight here:  

<details>
<summary>Client Machine</summary>

For Linux orchestrated servers, "Client Machine" should be the DNS name or IP address of the remote orchestrated server, while for Windows orchestratred servers, it should be the following URL format: protocol://dns-or-ip:port, where
* protocol is http or https, whatever your WinRM configuration uses
* dns-or-ip is the DNS name or IP address of the server
* port is the port WinRM is running under, usually 5985 for http and 5986 for https.

Example: https://myserver.mydomain.com:5986

If running as an agent (accessing stores on the server where the Universal Orchestrator Services is installed ONLY), Client Machine can be entered as stated above, OR you can bypass SSH/WinRM and access the local file system directly by adding "|LocalMachine" to the end of your value for Client Machine, for example "1.1.1.1|LocalMachine".  In this instance the value to the left of the pipe (|) is ignored.  It is important to make sure the values for Client Machine and Store Path together are unique for each certificate store created, as Keyfactor Command requires the Store Type you select, along with Client Machine, and Store Path together must be unique.  To ensure this, it is good practice to put the full DNS or IP Address to the left of the | character when setting up a cerificate store that will accessed without a WinRM/SSH connection.  

</details>

<details>
<summary>Store Path</summary>

For Linux orchestrated servers, "StorePath" will begin with a forward slash (/) and contain the full path and file name, including file extension if one exists (i.e. /folder/path/storename.ext).  For Windows orchestrated servers, it should be the full path and file name, including file extension if one exists, beginning with a drive letter (i.e. c:\folder\path\storename.ext).  

</details>

<details>
<summary>Server Username/Password</summary>

A username and password (or valid PAM key if the username and/or password is stored in a KF Command configured PAM integration).  The password can be an SSH private key if connecting via SSH to a server using SSH private key authentication.  If acting as an *agent* using local file access, just check "No Value" for the username and password.

</details>
<details>
<summary>Directories to Search</summary>

Enter one or more comma delimitted file paths to search (please reference the Keyfactor Command Reference Guide for more information), but there is also a special value that can be used on Windows orchestrated servers instead - "fullscan".  Entering fullscan in this field will tell the RemoteFile discovery job to search all available drive letters at the root and recursively search all of them for files matching the other search criteria.

</details>

<details>
<summary>Extensions</summary>

In addition to entering one or more comma delimitted extensions to search for (please reference the Keyfactor Command Reference Guide for more information), a reserved value of "noext" can be used that will cause the RemoteFile discovery job to search for files that do not have an extension.  This value can be chained with other extensions using the comma delimiter.  For example, entering pem,jks,noext will cause the RemoteFile discovery job to return file locations with extensions of "pem", "jks", *and* files that do not have extensions.  

</details>

Please refer to the Keyfactor Command Reference Guide for complete information on creating certificate stores and scheduling discovery jobs in Keyfactor Command.  
