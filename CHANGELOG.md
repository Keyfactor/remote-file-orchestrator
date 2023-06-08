v2.2.0
- Add ability to manage same windows server as installed without using WinRM
- Check for "core" version of PowerShell for command tweaks
- Bug fix: Preserve store permissions and file ownership when using separate upload file path

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
