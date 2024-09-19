## Overview

The RFPkcs12 store type can be used to manage any PKCS#12 compliant file format INCLUDING java keystores of type PKCS12.

Use cases supported:
1. One-to-many trust entries - A trust entry is considered single certificate without a private key in a certificate store.  Each trust entry is identified with a custom alias.
2. One-to-many key entries - One-to-many certificates with private keys and optionally the full certificate chain.  Each certificate identified with a custom alias.
3. A mix of trust and key entries.

## Requirements

{% include 'requirements.md' %}

## Configuration File Setup

{% include 'configurationfile.md' %}

## Discovery

{% include 'discovery.md' %}

## Client Machine Instructions

{% include 'clientmachine.md' %}

## Developer Notes

{% include 'developernotes.md' %}
