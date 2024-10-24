v2.9.0
- Modify Discovery on Linux servers to filter out ignored folders when searching using the Find command rather than eliminating them after.  This was done to eliminate permissions errors.
- Deprecated isRSAPrivateKey custom property on RFPEM certificate store type.  Integration now reads the existing private key to determin if it is formatted as PKCS#1 or PKCS#8 and, on renewal, keeps the format the same.  For new PEM certificate stores/certificates, PKCS#8 will always be used.  PLEASE NOTE, for existing certificate stores that already have isRSAPrivateKey defined, this setting will be ignored.
- Modified RFPkcs12 store type to handle single store certificate stores with no friendly name/alias
- Modify to create 2 builds - one for .net6 and one for .net8
- Update README to new DocTool format

v2.8.1
- Fixed issue were sensitive information could be exposed at debug logging level

v2.8.0
- Added new custom field - Remove Root Certificate from Chain - to allow adding certificate entries with the root CA certificate removed from the chain.
- Added SSH KeyboardInteractive Authentication support if Password Authentication is not enabled.

v2.7.0
- Modified RFJKS store type support java keystores of both PKCS12 and JKS
- Added support for OpenSSH private keys for SSH authentication
- Bug fix for orchestrators installed on Windows 2016
- Bug Fix: Supplied Linux user needing password reset could cause orchestrator locking.
- Bug Fix: Not supplying group for Linux File Owner on Store Creation caused the supplied owner to erroneously be used as the group for the newly create certificate store file.
- Updgraded Nuget packages for BouncyCastle and Renci.SSH.Net

v2.6.0
- Added ability for Linux installed universal orchestrator to manage stores as an "agent" (stores reside on same server as universal orchestrator) without the need to have SSH enabled.
- Added ability for Linux installed universal orchestrator to manage certificate stores on Windows servers by using SSH to communicate between the Linux UO server and the Windows machines hosting the certificate stores.
- Modified Discovery against Linux servers to use the -name option instead of -iname for the Linux shell "find" command, so Discovery will work for AIX servers.
- Upgraded several NuGet packages.

v2.5.0
- Add new optional custom field and config.json entries to supply a user id other than "root" for the user to "sudo into" when UseSudo = "Y".  There is an optional config.json DefaultSudoImpersonatedUser that will be used at the orchestrator level, and an optional new store type custom field, SudoImpersonatedUser, that overrides the config.json setting for each certificate store.
- Modified the optional sudo command prefix to remove the "-i" option which was creating a new shell for the impersonated id (always root up until this release).  Without this option the profile for the logged in user and not the impersonated user will be used when running commands.
- Added Regex checks for Discovery fields (file names, file extensions, and file paths) to enhance security.  Only alpha numeric, "/", and "\" characters are allowed for these values.

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
