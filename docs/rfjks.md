## RFJKS

The RFJKS Certificate Store Type is designed to manage Java Keystores of type JKS. This type facilitates the secure storage and management of cryptographic keys and certificates within the Java platform. By integrating RFJKS with the Remote File Universal Orchestrator extension, users can remotely inventory, add, remove, create, and discover certificates within JKS keystores, ensuring streamlined and efficient certificate management.

### Representation and Use Cases

The RFJKS Certificate Store represents a Java Keystore (JKS) format, which is widely used in Java applications for storing public and private keys and certificates. It supports various use cases, including the management of one-to-many trust entries, where each trust entry is identified with a custom alias, one-to-many key entries with private keys and optionally the full certificate chain, and a mix of trust and key entries.

### Important Considerations

There are several important caveats and limitations to be aware of when using the RFJKS Certificate Store Type:

1. **Type Specificity**: The RFJKS type strictly manages Java Keystores of type JKS. It cannot manage PKCS#12 formatted keystores. If you need to manage a PKCS#12 keystore, you must use the RFPkcs12 Certificate Store Type.

2. **Security Context**: When orchestrating management tasks, the security context is determined by the user ID entered in the Keyfactor Command certificate store or discovery job screens. When running locally as an agent, the security context is the user ID running the Keyfactor Command Universal Orchestrator service account.

3. **Credentials**: RFJKS requires a store password, but certificate stores can still be set up with no password by checking the "No Password" option during store creation in Keyfactor Command. This flexibility ensures compatibility with various security requirements.

4. **Linux Commands**: If managing keystores on a Linux server, it may be necessary to configure the orchestrated server so that the user can execute required Linux commands (e.g., chown, cp) with the appropriate permissions. This is particularly relevant if elevated access is required for certain tasks.

By understanding these key points, users can effectively leverage the RFJKS Certificate Store Type to manage Java Keystores within their Keyfactor Command environment, ensuring secure and efficient certificate handling.



### Supported Job Types

| Job Name | Supported |
| -------- | --------- |
| Inventory | ✅ |
| Management Add | ✅ |
| Management Remove | ✅ |
| Discovery | ✅ |
| Create | ✅ |
| Reenrollment |  |

## Requirements

### Prerequisites and Security Considerations

<details>
<summary><b>Certificate stores hosted on Linux servers:</b></summary>

1. The Remote File Orchestrator Extension makes use of a few common Linux commands when managing stores on Linux servers. If the credentials you will be connecting with need elevated access to run these commands or to access the certificate store files these commands operate against, you must set up the user id as a sudoer with no password necessary and set the config.json "UseSudo" value to "Y".  When RemoteFile is using orchestration, managing local or external certificate stores using SSH or WinRM, the security context is determined by the user id entered in the Keyfactor Command certificate store or discovery job screens.  When RemoteFile is running as an agent, managing local stores only, the security context is the user id running the Keyfactor Command Universal Orchestrator service account.  The full list of these commands below:

|Shell Command|Used For|
|---|---|
|echo|Used to append a newline and terminate all commands sent.|
|find|Used by Discovery jobs to locate potential certificate stores on the file system.|
|cp|Used by Inventory and Management Add/Remove jobs to copy the certificate store file to a temporary file (only when an alternate download folder has been configured).|
|chown|Used by the Inventory and Management Add/Remove jobs to set the permissions on the temporary file (only when an alternate download folder has been configured).|
|tee|Used by Management Add/Remove jobs to copy the temporary uploaded certificate file to the certificate store file (only when an alternate upload folder has been configured).|
|rm|Used by Inventory and Management Add/Remove jobs to remove temporary files (only when an alternate upload/download folder has been configured).|
|install|Used by the Management Create Store job when initializing a certificate store file.|
|orapki|Oracle Wallet CLI utility used by Inventory and Management Add/Remove jobs to manipulate an Oracle Wallet certificate store.  Used for the RFORA store type only.|
|gskcapicmd|IBM Key Database CLI utility used by Inventory and Management Add/Remove jobs to manipulate an IBM Key Database certificate store.  Used for the RFKDB store type only.|  

2. When orchestrating management of local or external certificate stores, the Remote File Orchestrator Extension makes use of SFTP and/or SCP to transfer files to and from the orchestrated server.  SFTP/SCP cannot make use of sudo, so all folders containing certificate stores will need to allow SFTP/SCP file transfer for the user assigned to the certificate store/discovery job.  If this is not possible, set the values in the config.json apprpriately to use an alternative upload/download folder that does allow SFTP/SCP file transfer.  If the certificate store/discovery job is configured for local (agent) access, the account running the Keyfactor Universal Orchestrator service must have access to read/write to the certificate store location, OR the config.json file must be set up to use the alternative upload/download file.  

3. SSH Key Authentication: When creating a Keyfactor certificate store for the remote file orchestrator extension, you may supply either a user id and password for the certificate store credentials (directly or through one of Keyfactor Command's PAM integrations), or supply a user id and SSH private key.  Both PKCS#1 (BEGIN RSA PRIVATE KEY) and PKCS#8 (BEGIN PRIVATE KEY) formats are supported for the SSH private key.  If using the normal Keyfactor Command credentials dialog without PAM integration, just copy and paste the full SSH private key into the Password textbox.  SSH Key Authentication is not available when running locally as an agent.

Please reference [Configuration File Setup](#configuration-file-setup) for more information on setting up the config.json file and [Certificate Stores and Discovery Jobs](#certificate-stores-and-discovery-jobs) for more information on the items above.    
</details>  

<details>  
<summary><b>Certificate stores hosted on Windows servers:</b></summary>
1. When orchestrating management of external (and potentially local) certificate stores, the RemoteFile Orchestrator Extension makes use of WinRM to connect to external certificate store servers.  The security context used is the user id entered in the Keyfactor Command certificate store or discovery job screen.  Make sure that WinRM is set up on the orchestrated server and that the WinRM port (by convention, 5585 for HTTP and 5586 for HTTPS) is part of the certificate store path when setting up your certificate stores/discovery jobs.  If running as an agent, managing local certificate stores, local commands are run under the security context of the user account running the Keyfactor Universal Orchestrator Service.  Please reference [Certificate Stores and Discovery Jobs](#certificate-stores-and-discovery-jobs) for more information on creating certificate stores for the RemoteFile Orchestrator Extension.  

</details>

Please consult with your company's system administrator for more information on configuring SSH/SFTP/SCP or WinRM in your environment.

  
&nbsp;

### Remote File Orchestrator Extension Installation
1. Review the [Prerequisites and Security Considerations](#prerequisites-and-security-considerations) section and make sure your environment is set up as required.
2. Refer to the [Creating Certificate Store Types](#creating-certificate-store-types) section to create the certificate store types you wish to manage.
3. Stop the Keyfactor Universal Orchestrator Service on the server you plan to install this extension to run on.
4. In the Keyfactor Orchestrator installation folder (by convention usually C:\Program Files\Keyfactor\Keyfactor Orchestrator for a Windows install or /opt/keyfactor/orchestrator/ for a Linux install), find the "Extensions" folder. Underneath that, create a new folder named "RemoteFile". You may choose to use a different name if you wish.
5. Download the latest version of the RemoteFile orchestrator extension from [GitHub](https://github.com/Keyfactor/remote-file-orchestrator).  Click on the "Latest" release link on the right hand side of the main page and download the first zip file.
6. Copy the contents of the download installation zip file to the folder created in step 4.
7. (Optional) If you decide to create one or more certificate store types with short names different than the suggested values, edit the manifest.json file in the folder you created in step 4, and modify each "ShortName" in each "Certstores.{ShortName}.{Operation}" line with the ShortName you used to create the respective certificate store type.
8. Modify the config.json file to use the settings you desire.  Please go to [Configuration File Setup](#configuration-file-setup) to learn more. 
9. Start the Keyfactor Universal Orchestrator Service.

</details>  

&nbsp;



## Certificate Store Type Configuration

The recommended method for creating the `RFJKS` Certificate Store Type is to use [kfutil](https://github.com/Keyfactor/kfutil). After installing, use the following command to create the `` Certificate Store Type:

```shell
kfutil store-types create RFJKS
```

<details><summary>RFJKS</summary>

Create a store type called `RFJKS` with the attributes in the tables below:

### Basic Tab
| Attribute | Value | Description |
| --------- | ----- | ----- |
| Name | RFJKS | Display name for the store type (may be customized) |
| Short Name | RFJKS | Short display name for the store type |
| Capability | RFJKS | Store type name orchestrator will register with. Check the box to allow entry of value |
| Supported Job Types (check the box for each) | Add, Discovery, Remove | Job types the extension supports |
| Supports Add | ✅ | Check the box. Indicates that the Store Type supports Management Add |
| Supports Remove | ✅ | Check the box. Indicates that the Store Type supports Management Remove |
| Supports Discovery | ✅ | Check the box. Indicates that the Store Type supports Discovery |
| Supports Reenrollment |  |  Indicates that the Store Type supports Reenrollment |
| Supports Create | ✅ | Check the box. Indicates that the Store Type supports store creation |
| Needs Server | ✅ | Determines if a target server name is required when creating store |
| Blueprint Allowed |  | Determines if store type may be included in an Orchestrator blueprint |
| Uses PowerShell |  | Determines if underlying implementation is PowerShell |
| Requires Store Password | ✅ | Determines if a store password is required when configuring an individual store. |
| Supports Entry Password |  | Determines if an individual entry within a store can have a password. |

The Basic tab should look like this:

![RFJKS Basic Tab](../docsource/images/RFJKS-basic-store-type-dialog.png)

### Advanced Tab
| Attribute | Value | Description |
| --------- | ----- | ----- |
| Supports Custom Alias | Required | Determines if an individual entry within a store can have a custom Alias. |
| Private Key Handling | Optional | This determines if Keyfactor can send the private key associated with a certificate to the store. Required because IIS certificates without private keys would be invalid. |
| PFX Password Style | Default | 'Default' - PFX password is randomly generated, 'Custom' - PFX password may be specified when the enrollment job is created (Requires the Allow Custom Password application setting to be enabled.) |

The Advanced tab should look like this:

![RFJKS Advanced Tab](../docsource/images/RFJKS-advanced-store-type-dialog.png)

### Custom Fields Tab
Custom fields operate at the certificate store level and are used to control how the orchestrator connects to the remote target server containing the certificate store to be managed. The following custom fields should be added to the store type:

| Name | Display Name | Type | Default Value/Options | Required | Description |
| ---- | ------------ | ---- | --------------------- | -------- | ----------- |


The Custom Fields tab should look like this:

![RFJKS Custom Fields Tab](../docsource/images/RFJKS-custom-fields-store-type-dialog.png)



</details>

## Certificate Store Configuration

After creating the `RFJKS` Certificate Store Type and installing the Remote File Universal Orchestrator extension, you can create new [Certificate Stores](https://software.keyfactor.com/Core-OnPrem/Current/Content/ReferenceGuide/Certificate%20Stores.htm?Highlight=certificate%20store) to manage certificates in the remote platform.

The following table describes the required and optional fields for the `RFJKS` certificate store type.

| Attribute | Description | Attribute is PAM Eligible |
| --------- | ----------- | ------------------------- |
| Category | Select "RFJKS" or the customized certificate store name from the previous step. | |
| Container | Optional container to associate certificate store with. | |
| Client Machine | The Client Machine field for Certificate Stores of type 'RFJKS' should be the DNS name or IP address of the remote orchestrated Linux or Windows server. Example for Windows: 'https://myserver.mydomain.com:5986'. | |
| Store Path | The Store Path field for Certificate Stores of type 'RFJKS' should include the full path and file name, including the file extension if one exists. Example for Linux: '/folder/path/storename.jks'. | |
| Orchestrator | Select an approved orchestrator capable of managing `RFJKS` certificates. Specifically, one with the `RFJKS` capability. | |

* **Using kfutil**

    ```shell
    # Generate a CSV template for the AzureApp certificate store
    kfutil stores import generate-template --store-type-name RFJKS --outpath RFJKS.csv

    # Open the CSV file and fill in the required fields for each certificate store.

    # Import the CSV file to create the certificate stores
    kfutil stores import csv --store-type-name RFJKS --file RFJKS.csv
    ```

* **Manually with the Command UI**: In Keyfactor Command, navigate to Certificate Stores from the Locations Menu. Click the Add button to create a new Certificate Store using the attributes in the table above.