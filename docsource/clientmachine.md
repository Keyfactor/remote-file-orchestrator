When creating a Certificate Store or scheduling a Discovery Job, you will be asked to provide a "Client Machine".

For Linux orchestrated servers, "Client Machine" should be the DNS name or IP address of the remote orchestrated server, while for Windows orchestratred servers, it should be the following URL format: protocol://dns-or-ip:port, where
* protocol is http or https, whatever your WinRM configuration uses
* dns-or-ip is the DNS name or IP address of the server
* port is the port WinRM is running under, usually 5985 for http and 5986 for https.

Example: https://myserver.mydomain.com:5986

If running as an agent (accessing stores on the server where the Universal Orchestrator Services is installed ONLY), Client Machine can be entered as stated above, OR you can bypass SSH/WinRM and access the local file system directly by adding "|LocalMachine" to the end of your value for Client Machine, for example "1.1.1.1|LocalMachine".  In this instance the value to the left of the pipe (|) is ignored.  It is important to make sure the values for Client Machine and Store Path together are unique for each certificate store created, as Keyfactor Command requires the Store Type you select, along with Client Machine, and Store Path together must be unique.  To ensure this, it is good practice to put the full DNS or IP Address to the left of the | character when setting up a cerificate store that will accessed without a WinRM/SSH connection.  