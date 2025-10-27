## Overview

The Remote File Orchestrator Extension is a multipurpose integration that can remotely manage a variety of file-based
certificate stores and can easily be extended to manage others.

The Keyfactor Universal Orchestrator (UO) and RemoteFile Extension can be installed on either Windows or Linux operating
systems as well as manage certificates residing on servers of both operating systems. A UO service managing certificates
on remote servers is considered to be acting as an Orchestrator, while a UO service managing local certificates on the
same server running the service is considered an Agent. When acting as an Orchestrator, connectivity from the
orchestrator server hosting the `RemoteFile` extension to the orchestrated server hosting the certificate store(s) being
managed is achieved via either an `SSH` (for Linux and possibly Windows orchestrated servers) or WinRM (for Windows
orchestrated servers) connection. When acting as an agent, `SSH/WinRM` may still be used, OR the certificate store can be
configured to bypass these and instead directly access the orchestrator server's file system.

![](images/orchestrator-agent.png)

The supported configurations of Universal Orchestrator hosts and managed orchestrated servers are detailed below:

|                                                                           | UO Installed on Windows               | UO Installed on Linux               |
|---------------------------------------------------------------------------|---------------------------------------|-------------------------------------|
| Orchestrated Server hosting certificate store(s) on remote Windows server | WinRM connection                      | SSH connection                      |
| Orchestrated Server hosting certificate store(s) on remote Linux server   | SSH connection                        | SSH connection                      |
| Certificate store(s) on same server as orchestrator service (Agent)       | WinRM connection or local file system | SSH connection or local file system |  

Note: when creating, adding certificates to, or removing certificates from any store managed by `RemoteFile`, the
destination store file will be recreated. When this occurs, current AES encryption algorithms will be used for affected
certificates and certificate store files.

## Requirements

<details>
<summary><b>Certificate stores hosted on Linux servers:</b></summary>

1. The Remote File Orchestrator Extension makes use of a few common Linux commands when managing stores on Linux
   servers as well as some specialized CLI commands for certain store types. If the credentials you will be connecting with 
   need elevated access to run these commands or to access the
   certificate store files these commands operate against, you must set up the user id as a sudoer with no password
   necessary and set the config.json `UseSudo` value to `Y`. When `RemoteFile` is using orchestration, managing local or
   external certificate stores using `SSH` or `WinRM`, the security context is determined by the user id entered into the
   Keyfactor Command certificate store or discovery job screens. When RemoteFile is running as an agent, managing local
   stores only, the security context is the user id running the Keyfactor Command Universal Orchestrator service
   account. The full list of these commands and when they are used is illustrated below:

| Shell Command  | Discovery | Inventory | Management-Add | Management-Delete | Management-Create |
|----------------|-----------|-----------|----------------|-------------------|-------------------|
| `echo`         | X         | X         | X              | X                 | X                 |
| `find`         | X         |           |                |                   |                   |
| `cp`           |           | X(a)      | X(a)           | X(a)              |                   |
| `ls`           |           |           | X              | X                 | X                 |
| `chown`        |           | X(b)      | X(b)           | X(b)              |                   |
| `tee`          |           | X(c)      | X(a)           | X(a)              |                   |
| `rm`           |           | X(d)      | X(d)           | X(d)              |                   |
| `install`      |           |           |                |                   | X                 |
| `orapki`       |           | X(e)      | X(e)           | X(e)              |                   |
| `gskcapicmd`   |           | X(f)      | X(f)           | X(f)              |                   |  

(a) - Only used if [config.json](#post-installation) setting SeparateUploadFilePath is used (non empty value)  
(b) - Only used if [config.json](#post-installation) setting SeparateUploadFilePath is used (non empty value) AND the [config.json](#post-installation) or certificate store setting SudoImpersonatedUser is not used (empty value)  
(c) - Only used if store type is RFKDB or RFORA AND [config.json](#post-installation) setting SeparateUploadFilePath is used (non empty value)  
(d) - Only used if using store type is either RFKDB or RFORA OR any store type and the [config.json](#post-installation) setting SeparateUploadFilePath is used (non empty value)  
(e) - RFORA store type only  
(f) - RFKDB store type only

2. When orchestrating management of local or external certificate stores, the Remote File Orchestrator Extension makes
   use of SCP or SFTP to transfer files to and from the orchestrated server. SCP is attempted first, and if that 
   fails, SFTP is attempted. `SCP/SFTP` cannot make use of `sudo`, so
   all folders containing certificate stores will need to allow SCP/SFTP file transfer for the user assigned to the
   certificate store/discovery job. If this is not possible, set the values in the `config.json` appropriately to use an
   alternative upload/download folder that does allow `SCP/SFTP` file transfer. If the certificate store/discovery job is
   configured for local (agent) access, the account running the Keyfactor Universal Orchestrator service must have
   access to read/write to the certificate store location, OR the `config.json` file must be set up to use the alternative
   upload/download file.

3. `SSH` Authentication: When creating a Keyfactor certificate store for the `RemoteFile` orchestrator extension, you may
   supply either a user id and password for the certificate store credentials (directly or through one of Keyfactor
   Command's PAM integrations), or supply a user id and `SSH` private key. When using a password, the connection is
   attempted using `SSH` password authentication. If that fails, Keyboard Interactive Authentication is automatically
   attempted. One or both of these must be enabled on the Linux box being managed. If private key authentication is
   desired, copy and paste the full SSH private key into the Password textbox (or pointer to the private key if using a
   PAM provider). Please note that SSH Private Key Authentication is not available when running locally as an agent. The
   following private key formats are supported:

- PKCS#1 (`BEGIN RSA PRIVATE KEY`)
- PKCS#8 (`BEGIN PRIVATE KEY`)
- ECDSA OPENSSH (`BEGIN OPENSSH PRIVATE KEY`)

Please reference [Post Installation](#post-installation) for more information on setting up the `config.json` file
and [Defining Certificate Stores](#defining-certificate-stores)
and [Discovering Certificate Stores with the Discovery Job](#discovering-certificate-stores-with-the-discovery-job) for
more information on defining and configuring certificate stores.
</details>  

<details>  
<summary><b>Certificate stores hosted on Windows servers:</b></summary>

1. When orchestrating management of external (and potentially local) certificate stores, the `RemoteFile` Orchestrator 
Extension makes use of `WinRM` to connect to external certificate store servers.  The security context used is the user id 
entered in the Keyfactor Command certificate store or discovery job screen.  Make sure that `WinRM` is set up on the 
orchestrated server and that the `WinRM` port (by convention, `5585` for `HTTP` and `5586` for `HTTPS`) is part of the certificate 
store path when setting up your certificate stores/discovery jobs. If running as an agent, managing local certificate stores, 
local commands are run under the security context of the user account running the Keyfactor Universal Orchestrator Service.  
Please reference [Certificate Stores and Discovery Jobs](#certificate-stores-and-discovery-jobs) for more information on 
creating certificate stores for the `RemoteFile` Orchestrator Extension.  

</details>
C
Please consult with your system administrator for more information on configuring `SSH/SCP/SFTP` or `WinRM` in your environment.

## Post Installation

The Remote File Orchestrator Extension uses a JSON configuration file. It is located at `{Keyfactor Orchestrator Installation Folder}\Extensions\RemoteFile\config.json`. None of the values are required, and a description of each follows below:

```json
{
  "UseSudo": "N",
  "DefaultSudoImpersonatedUser": "",
  "CreateStoreIfMissing": "N",
  "UseNegotiate": "N",
  "SeparateUploadFilePath": "",
  "DefaultLinuxPermissionsOnStoreCreation": "600",
  "DefaultOwnerOnStoreCreation": "",
  "SSHPort": "",
  "UseShellCommands":  "Y"
}
``` 

| Key                                      | Default Value | Allowed Values                        | Description                                                                                                                                                                                                                                              |
|------------------------------------------|---------------|---------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `UseSudo`                                | `N`           | `Y/N`                                 | Determines whether to prefix Linux commands with `sudo`. Setting to `Y` will prefix all Linux commands with `sudo`. Setting to `N` will not add `sudo` to Linux commands. Only applicable for Linux hosted certificate stores.                           |
| `DefaultSudoImpersonatedUser`            |               | Any valid user id                     | Used with UseSudo=`Y` to set an alternate user to impersonate with sudo. If empty, `root` will be used by default. The user must have permissions to SCP/SFTP files and execute necessary commands. Only applicable for Linux hosted certificate stores. |
| `CreateStoreIfMissing`                   | `N`           | `Y/N`                                 | Determines if a certificate store should be created during a Management-Add job if it doesn't exist. If `N`, the job will return an error. If `Y`, the store will be created and the certificate added.                                                  |
| `UseNegotiate`                           | `N`           | `Y/N`                                 | Determines if WinRM should use Negotiate (Y) when connecting to the remote server. Only applicable for Windows hosted certificate stores.                                                                                                                |
| `SeparateUploadFilePath`                 |               | Any valid, existing Linux path        | Path on the orchestrated server for uploading/downloading temporary work files. If empty, the certificate store location will be used. Only applicable for Linux hosted certificate stores.                                                              |
| `DefaultLinuxPermissionsOnStoreCreation` | `600`         | Any 3-digit value from 000-777        | Linux file permissions set on new certificate stores. If blank, permissions from the parent folder will be used. Only applicable for Linux hosted certificate stores.                                                                                    |
| `DefaultOwnerOnStoreCreation`            |               | Any valid user id                     | Sets the owner for newly created certificate stores. Can include group with format `ownerId:groupId`. If blank, the owner of the parent folder will be used. Only applicable for Linux hosted certificate stores.                                        |
| `SSHPort`                                |               | Any valid integer representing a port | The port that SSH is listening on. Default is 22. Only applicable for Linux hosted certificate stores.                                                                                                                                                   |
| `UseShellCommands`                       | `Y`           | `Y/N`                                 | Recommended to be set to the default value of 'Y'.  For a detailed explanation of this setting, please refer to [Use Shell Commands Setting](#use-shell-commands-setting)                                                                                |

## Discovery

When scheduling discovery jobs in Keyfactor Command, there are a few fields that are important to highlight here:

| Field                    | Description                                                                                                                                                                                                                                                                                                                                           |
|--------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Client Machine           | The IP address or DNS of the server hosting the certificate store. For more information, see [Client Machine](#client-machine-instructions)                                                                                                                                                                                                           |
| Server Username/Password | A username and password (or valid PAM key if the username and/or password is stored in a KF Command configured PAM integration). The password can be an SSH private key if connecting via SSH to a server using SSH private key authentication. If acting as an *agent* using local file access, just check `No Value` for the username and password. |
| Directories to Search    | Enter one or more comma delimited file paths to search. A special value `fullscan` can be used on Windows orchestrated servers to search all available drive letters at the root and recursively search all of them for files matching the other search criteria.                                                                                     |
| Extensions               | Enter one or more comma delimited extensions to search for. A reserved value of `noext` can be used to search for files without an extension. This can be combined with other extensions (e.g., pem,jks,noext will find files with .pem and .jks extensions, as well as files with no extension).                                                     |

Please refer to the Keyfactor Command Reference Guide for complete information on creating certificate stores and
scheduling discovery jobs in Keyfactor Command.

## Client Machine Instructions

When creating a Certificate Store or scheduling a Discovery Job, you will be asked to provide a `Client Machine`.

### Linux

For Linux orchestrated servers, `Client Machine` should be the DNS name or IP address of the remote orchestrated server.

### Windows

For Windows orchestrated servers, it should be the following URL format: `protocol://dns-or-ip:port`.

| Component   | Description                               | Common Value                  |
|-------------|-------------------------------------------|-------------------------------|
| `protocol`  | Protocol used by your WinRM configuration | http or https                 |
| `dns-or-ip` | DNS name or IP address of the server      | domain name or IP address     |
| `port`      | Port WinRM is running under               | 5985 for http, 5986 for https |

Example: https://myserver.mydomain.com:5986

### Localhost

For agent mode (accessing stores on the same server where Universal Orchestrator Services is running):

1. Add `|LocalMachine` to the Client Machine value to bypass SSH/WinRM and access the local file system directly
    - Examples: `1.1.1.1|LocalMachine`, `hostname|LocalMachine`
    - The value to the left of the pipe `|` will not be used for connectivity but will be used as part of the
      Certificate Store definition in Keyfactor Command

2. Important considerations:
    - `Store Type` + `Client Machine` + `Store Path` must be unique in Keyfactor Command
    - Best practice: Use the full DNS or IP Address to the left of the `|` character


## Use Shell Commands Setting

The Use Shell Commands Setting (orchestrator level in config.json and per store override of this value as a custom field value) 
determines whether or not Linux shell commands will be used when managing certificate stores on Linux-based servers.
This is useful for environments where shell access is limited or even not allowed.  In those scenarios setting this value to 'N'
will substitute SFTP commands for certain specific Linux shell commands.  The following restrictions will be in place when 
using RemoteFile in this mode:
1. The config.json option SeparateUploadFilePath must NOT be used (option missing from the config.json file or set to empty) for shell
commands to be suppressed for all use cases.
2. The config.json and custom field options DefaultLinuxPermissionsOnStoreCreation, DefaultOwnerOnStoreCreation, 
LinuxFilePermissionsOnStoreCreation, and LinuxFileOwnerOnStoreCreation are not supported and will be ignored.  As a result, file
permissions and ownership when creating certificate stores will be based on the user assigned to the Command certificate store and 
other Linux environmental settings.
3. Discovery jobs are excluded and will still use the `find` shell command
4. A rare issue exists where the user id assigned to a certificate store has an expired password causing the orchestrator to hang 
when attempting an SCP/SFTP connection.  A modification was added to RemoteFile to check for this condition.  Running RemoteFile 
with Use Shell Commands = N will cause this validation check to NOT occur.
5. Both RFORA and RFKDB use proprietary CLI commands in order to manage their respective certificate stores.  These commands
will still be executed when Use Shell Commands is set to Y.
6. If executing in local mode ('|LocalMachine' at the end of your client machine name for your certificate store), Use Shell
Commands = 'N' will have no effect.  Shell commands will continue to be used because there will be no SSH connection
available from which to execute SFTP commands.


## Developer Notes

The Remote File Orchestrator Extension is designed to be highly extensible, enabling its use with various file-based
certificate stores beyond the specific implementations currently referenced above. The advantage to extending this
integration rather than creating a new one is that the configuration, remoting, and Inventory/Management/Discovery logic
is already written. The developer needs to only implement a few classes and write code to convert the desired file-based 
store to a common format. This section describes the steps necessary to add additional store/file types. Please
note that familiarity with the [.Net Core BouncyCastle cryptography library](https://github.com/bcgit/bc-csharp) is a
prerequisite for adding additional supported file/store types.

Steps to create a new supported file-based certificate store type:

1. Clone this repository from GitHub
2. Open the .net core solution in the IDE of your choice
3. Under the ImplementationStoreTypes folder, create a new folder named for the new certificate store type
4. Create a new class (with namespace of Keyfactor.Extensions.Orchestrator.RemoteFile.{NewType}) in the new folder that
   will implement ICertificateStoreSerializer. By convention, {StoreTypeName}CertificateSerializer would be a good
   choice for the class name. This interface requires you to implement three methods:
    - `DesrializeRemoteCertificateStore` - This method takes in a byte array containing the contents of file-based store
      you are managing. The developer will need to convert that to an Org.BouncyCastle.Pkcs.Pkcs12Store class and return
      it.
    - `SerializeRemoteCertificateStore` - This method takes in an Org.BouncyCastle.Pkcs.Pkcs12Store and converts it to a
      collection of custom file representations.
    - `GetPrivateKeyPath` - This method returns the location of the external private key file for single certificate
      stores. This is only used for `RFPEM`, and all other implementations return `NULL` for this method. If this
      is not applicable to your implementation, just return a `NULL` value for this method.
5. Create an `Inventory.cs` class (with namespace of `Keyfactor.Extensions.Orchestrator.RemoteFile.{NewType}`) under the new
   folder and have it inherit `InventoryBase`. Override the internal `GetCertificateStoreSerializer()` method with a one-line implementation returning a new instantiation of the class created in step 4.
6. Create a `Management.cs` class (with namespace of `Keyfactor.Extensions.Orchestrator.RemoteFile.{NewType}`) under the new
   folder and have it inherit `ManagementBase`. Override the internal `GetCertificateStoreSerializer()` method with a one-line implementation returning a new instantiation of the class created in step 4.
7. Modify the manifest.json file to add three new sections (for Inventory, Management, and Discovery). Make sure for
   each, the `NewType` in Certstores.{NewType}.{Operation}, matches what you will use for the certificate store type
   short name in Keyfactor Command. On the `TypeFullName` line for all three sections, make sure the namespace matches
   what you used for your new classes. Note that the namespace for Discovery uses a common class for all supported
   types. Discovery is a common implementation for all supported store types.
8. Modify the integration-manifest.json file to add the new store type under the store_types element.