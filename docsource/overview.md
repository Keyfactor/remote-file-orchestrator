## Overview

The Remote File Universal Orchestrator extension is a versatile integration that facilitates the remote management of various file-based certificate stores. This extension is capable of managing multiple store types, providing comprehensive support for certificates in different formats. The supported certificate store types include:

- **RFPkcs12**: Manages any PKCS#12 compliant file format, including Java keystores of type PKCS12. It supports one-to-many trust entries, one-to-many key entries, and a mix of trust and key entries.
- **RFJKS**: Manages Java keystores of type JKS. Unlike RFPkcs12, this type strictly supports Java keystores of type JKS and not PKCS12.
- **RFPEM**: Manages PEM encoded files. This store type supports trust stores, single certificate stores with a private key in the file, single certificate stores with a certificate chain and private key in the file, and configurations with external private key files.
- **RFDER**: Manages DER encoded files, supporting single certificate stores with an external private key file and without a private key.
- **RFKDB**: Manages IBM Key Database Files (KDB). Requires the IBM utility, GSKCAPICMD, to be installed on the server where the KDB file resides. Supports one-to-many trust entries, key entries, and a mix of both.
- **RFORA**: Manages Pkcs12 Oracle Wallets, primarily tested on wallets installed on Windows. It supports one-to-many trust entries, key entries, and a mix of both.

Each of these certificate store types represents a unique format or structure for storing cryptographic certificates and keys. The main differences between them lie in the specific file format they manage and the use cases they support. For instance, RFPkcs12 can manage both trust and key entries, while RFPEM is highly versatile with its support for both internal and external private key files. In contrast, RFDER is limited to single certificate stores with optional external private keys.

The Keyfactor Universal Orchestrator (UO) and Remote File Extension can be installed on Windows or Linux operating systems, and manage certificates on servers of both types. When running on a remote server, the extension acts as an Orchestrator, connecting via SSH (for Linux) or WinRM (for Windows). When running locally on the same server, it acts as an Agent, potentially bypassing SSH/WinRM and directly accessing the file system.

By integrating closely with Keyfactor Command, the Remote File Universal Orchestrator extension simplifies and automates the management of certificates across diverse environments and certificate store types.

