v2.4.2
- Bug fix: Upgrade BouncyCastle.Cryptography to version 2.3.0 to allow for RFKDB HMAC-SHA-384 support

v2.4.1
- Fix logging issue for RFKDB

v2.4.0
- Add new optional custom parameter, IgnorePrivateKeyOnInventory, for RFPEM, which will allow inventorying RFPEM certificate stores where the store password is unknown.  This will make the store INVENTORY ONLY.  Once the store password is added, this option can be de-selected (set to False), inventory can be run again, and then renewing/removing the certificate will be allowed.
- Bug fix: Discovery "Directories to Ignore" field not being used to filter results

v2.3.1
- Bug fix: Discovery - ignore /proc folder for Linux servers

v2.3.0
- New RFORA store type for Oracle Wallet support
- Add ability to set separate owner and group id's when creating certificate stores.
- Bug fix: "noext" extension option for Discovery on Windows servers
- Bug fix: Added parentheses as valid characters for store path on Windows servers.

v2.2.0
- Add ability to manage same windows server as installed without using WinRM
- Check for "core" version of PowerShell for command tweaks
- Bug fix: Preserve store permissions and file ownership when using separate upload file path
- Bug fix: Fixed issue adding certificates to stores with embedded spaces in path (Windows managed stores only)

v2.1.2
- Bug fix: Discovery not working against Windows servers
- Bug fix: Issue running Discovery on Windows servers with one or more spaces in the path

v2.1
- New RFDER certificate store type added
- RFPEM modified to now support PKCS#1 private key formats (BEGIN RSA PRIVATE KEY)
- Added support for rsa-sha2-256 HostKeyAlgorithm for SSH "handshake" when connecting to Linux managed servers
- Added new optional certificate store type custom field to specify separate file owner when creating certificate stores during Management-Create jobs
- Bug fix: Java-based applications were not recognizing trust entries added to java keystores of type PKCS12
- Bug fix: File download operations for Windows managed servers were still using the credentials from the orchestrator service instead of from the certificate store

v2.0
- Added PAM support

v1.1
- Added support for IBM Key Database (KDB) files
- Extended error messaging for SSH/SFTP/SCP connection issues

v1.0
- Initial Version
