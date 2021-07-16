# CyLR

[![Build Status](https://travis-ci.com/orlikoski/CyLR.svg?branch=main)](https://travis-ci.com/orlikoski/CyLR)

CyLR — Live Response Collection tool by Alan Orlikoski and Jason Yegge
This page is a fork of https://github.com/orlikoski/CyLR, initially to continue the pre-NET.core build.

## Please Read

[Open Letter to the users of Skadi, CyLR, and CDQR](https://docs.google.com/document/d/1L6CBvFd7d1Qf4IxSJSdkKMTdbBuWzSzUM3u_h5ZCegY/edit?usp=sharing)

## Videos and Media

* [OSDFCON 2017](http://www.osdfcon.org/presentations/2017/Asif-Matadar_Rapid-Incident-Response.pdf)
  Slides: Walk-through different techniques that are required to provide
  forensics results for Windows and *nix environments (Including CyLR and CDQR)

## What is CyLR

The CyLR tool collects forensic artifacts from hosts with NTFS file systems
quickly, securely and minimizes impact to the host.

The main features are:

* Quick collection (it's really fast)
* Raw file collection process does not use Windows API
* Collection of key artifacts by default.
* Ability to specify custom targets for collection.
* Acquisition of special and in-use files, including alternate data streams,
  system files, and hidden files.
* Glob and regular expression patterns are available to specify custom targets.
* Data is collected into a zip file, allowing the user to modify the compression
  level, set an archive password, and file name.
* Specification of a SFTP destination for the file archive.

CyLR uses .NET Core and runs natively on Windows, Linux, and MacOS. Self
contained applications for the following are included in releases for
version 2.0 and higher.

* Windows x86
* Windows x64
* Linux x64
* MacOS x64

## SYNOPSIS

Below is the output of CyLR:

```text
$ CyLR -h
CyLR Version 1.6.8

Usage: CyLR [Options]... [Files]...

The CyLR tool collects forensic artifacts from hosts with NTFS file systems
quickly, securely and minimizes impact to the host.

The available options are:
-od
        Defines the directory that the zip archive will be created in.
        Defaults to current working directory.
        Usage: -od <directory path>
-of
        Defines the name of the zip archive will be created. Defaults to
        host machine's name.
        Usage: -of <archive name> (exclude.zip)
        
-drive
        Optional point collector at a different drive letter for collection.
        Useful when collecting triage image from mounted volumes VHDX, VMDK, etc
        Usage: -drive G:
-c
        Optional argument to provide custom list of artifact files and
        directories (one entry per line). NOTE: Please see
        CUSTOM_PATH_TEMPLATE.txt for sample.
        Usage: -c <path to config file>
-d
        Same as '-c' but will collect default paths included in CyLR in
        addition to those specified in the provided config file.
        Usage: -d <path to config file>
-u
        SFTP username
        Usage: -u <sftp-username>
-p
        SFTP password
        Usage: -p <password>
-s
        SFTP Server resolvable hostname or IP address and port. If no port
        is given then 22 is used by default.  Format is <server name>:<port>
        Usage: -s <ip>:<port>
-os
        Defines the output directory on the SFTP server, as it may be a
        different location than the ZIP generate on disk. Can be full or
        relative path.
        Usage: -os <directory path>
-zp
        If specified, the resulting zip file will be password protected
        with this password.
        Usage: -zp <password>
-zl
        Uses a number between 1-9 to change the compression level
        of the archive file. Defaults to 3
        Usage: -zl <0-9>
--no-sftpcleanup
        Disables the removal of the .zip file used for collection after
        uploading to the SFTP server. Only applies if SFTP option is enabled.
        Usage: --no-sftpcleanup
--dry-run
        Collect artifacts to a virtual zip archive, but does not send
        or write to disk.
--force-native
        Uses the native file system instead of a raw NTFS read. Unix-like
        environments always use this option.
--no-desktop
        Disables collection of \Users\*\Desktop\*
--no-othermft
        Disables collection of MFT and other NTFS artefacts from drives other
        than the primary drive (ie C:)     
--no-usnjrnl
        Disables collecting $UsnJrnl
-- recycle
        Enables collection of the Recycle Bin

-l
        Sets the file path to write log messages to. Defaults to ./CyLR.log
        Usage: -l CyLR_run.log
-q
        Disables logging to the console and file.
        Usage: -q
-v
        Increases verbosity of the console log. By default the console
        only shows information or greater events and the file log shows
        all entries. Disabled when `-q` is used.
        Usage: -v
```

## Default Collection Paths

CyLR tool collects forensic artifacts from hosts with NTFS file systems
quickly, securely and minimizes impact to the host. All collection paths are
case-insensitive.

**Note:** See CollectionPaths.cs for a full list of default files collected and
for the underlying patterns used for collection. You can easily extend this list
through the use of patterns as shown in CUSTOM_PATH_TEMPLATE.txt or by opening
a pull request.

The standard list of collected artifacts are as follows:

### Windows

From every drive on the machine (unless otherwise specified):
* `\$LogFile`
* `\$MFT`
* `\$secure:$SDS`
* `\$Boot`
* `\$Extend\$UsnJrnl:$J`
* `\Extend\$UsnJrnl:$Max`
* `\Extend\$RmMetadata\$TxfLog\$Tops:$T`

System Drive (ie C:)

* `\inetpub\logs` // add in IIS logs
* `\ProgramData\Microsoft\Diagnosis\EventTranscript\EventTranscript.db` // added 16.07.2021 based on https://www.kroll.com/en/insights/publications/cyber/forensically-unpacking-eventtranscript
* `\ProgramData\Microsoft\Search\Data\Applications\Windows`
* `\ProgramData\Microsoft\Network\Downloader`
* `\ProgramData\Microsoft\RAC\PublishedData`
* `\ProgramData\Microsoft\Windows\Start Menu\Programs\Startup`
* `\Program Files (x86)\TeamViewer\Connections_incoming.txt`
* `\Program Files\TeamViewer\Connections_incoming.txt`
* `\Windows\Appcompat\Programs\install`
* `\Windows\Appcompat\Programs\Amcache.hve`
* `\Windows\Appcompat\Programs\Amcache.hve.LOG1`
* `\Windows\Appcompat\Programs\Amcache.hve.LOG2`
* `\Windows\Appcompat\Programs\Amcache.hve.tmp.LOG1`
* `\Windows\Appcompat\Programs\Amcache.hve.tmp.LOG2`
* `\Windows\Appcompat\Programs\recentfilecache.bcf`
* `\Windows\System Volume Information\syscache.hve`
* `\Windows\System Volume Information\syscache.hve.LOG1`
* `\Windows\System Volume Information\syscache.hve.LOG2`
* `\Windows\inf\setupapi.dev.log`
* `\Windows\Logs`
* `\Windows\NTDS` // add in collection of Active Directory NTDS folder
* `\Windows\Prefetch`
* `\Windows\SchedLgU.Txt`
* `\Windows\ServiceProfiles\LocalService`
* `\Windows\ServiceProfiles\NetworkService`
* `\Windows\System32\config`
* `\Windows\System32\config\SAM.LOG1`
* `\Windows\System32\config\SAM.LOG2`
* `\Windows\System32\config\SECURITY.LOG1`
* `\Windows\System32\config\SECURITY.LOG2`
* `\Windows\System32\config\SOFTWARE.LOG1`
* `\Windows\System32\config\SOFTWARE.LOG2`
* `\Windows\System32\config\SYSTEM.LOG1`
* `\Windows\System32\config\SYSTEM.LOG2`
* `\Windows\System32\dhcp` // add in collection of DHCP logs and database
* `\Windows\System32\dns` // add in collection of DNS logs and database
* `\Windows\System32\drivers\etc\hosts`
* `\Windows\System32\LogFiles`
* `\Windows\System32\bits.log` // add in BITS transfer logs
* `\Windows\System32\sru`
* `\Windows\System32\Tasks`
* `\Windows\System32\wbem\Repository`
* `\Windows\System32\winevt\logs`
* `\Windows\Tasks`

User Profiles (ie `C:\Users\*`):

* `\Users\*\NTUSER.DAT`
* `\Users\*\NTUSER.DAT.LOG1`
* `\Users\*\NTUSER.DAT.LOG2`
* `\Users\*\Desktop`
* `\Users\*\AppData\Local\Putty.rnd`
* `\Users\*\AppData\Local\ConnectedDevicesPlatform`
* `\Users\*\AppData\Local\Google\Chrome\User Data`
* `\Users\*\AppData\Local\Google\Chrome SxS\User Data`
* `\Users\*\AppData\Local\Microsoft\Edge`
* `\Users\*\AppData\Local\Microsoft\Terminal Service Client\Cache`
* `\Users\*\AppData\Local\Microsoft\Windows\Internet Explorer`
* `\Users\*\AppData\Local\Microsoft\Windows\Cookies`
* `\Users\*\AppData\Local\Microsoft\Windows\Explorer`
* `\Users\*\AppData\Local\Microsoft\Windows\IEDownloadHistory`
* `\Users\*\AppData\Local\Microsoft\Windows\INetCache`
* `\Users\*\AppData\Local\Microsoft\Windows\INetCookies`
* `\Users\*\AppData\Local\Microsoft\Windows\History`
* `\Users\*\AppData\Local\Microsoft\Windows\Temporary Internet Files`
* `\Users\*\AppData\Local\Microsoft\Windows\UsrClass.dat`
* `\Users\*\AppData\Local\Microsoft\Windows\UsrClass.dat.LOG1`
* `\Users\*\AppData\Local\Microsoft\Windows\UsrClass.dat.LOG2`
* `\Users\*\AppData\Local\Microsoft\Windows\WebCache`
* `\Users\*\AppData\Local\Mozilla\Firefox\Profiles`
* `\Users\*\AppData\Local\Packages\Microsoft.MicrosoftEdge_8wekyb3d8bbwe`
* `\Users\*\AppData\Roaming\winscp.ini`
* `\Users\*\AppData\Roaming\winscp.rnd`
* `\Users\*\AppData\Roaming\Google\Chrome\User Data` // added 03.07.2021 after finding Google Chrome data in Roaming not Local on a case
* `\Users\*\AppData\Roaming\Google\Chrome SxS\User Data` // added in Google Chrome Canary (dev version)
* `\Users\*\AppData\Roaming\Opera`
* `\Users\*\AppData\Roaming\Microsoft\Internet Explorer`
* `\Users\*\AppData\Roaming\Microsoft\Windows\Office\Recent`
* `\Users\*\AppData\Roaming\Microsoft\Windows\PowerShell\PSReadline`
* `\Users\*\AppData\Roaming\Microsoft\Windows\Recent`
* `\Users\*\AppData\Roaming\Microsoft\Windows\Start Menu`
* `\Users\*\AppData\Roaming\Mozilla\Firefox\Profiles`
* `\Users\*\AppData\Roaming\TeamViewer`

## EXAMPLES

### Standard collection

```text
CyLR.exe
```

### Linux/macOS collection

```text
./CyLR
```

### Collect artifacts and store data in "C:\Temp\LRData"

```text
CyLR.exe -od "C:\Temp\LRData"
```

### Collect artifacts and store data in ".\LRData"

```text
CyLR.exe -od LRData
```

### Disable log file

```text
CyLR.exe -q
```

### Collect artifacts and send data to SFTP server 8.8.8.8

```text
CyLR.exe -u username -p password -s 8.8.8.8
```

### Collect to another folder and filename ie data\important-YYYYmmdd-hhmmss.zip

```text
CyLR -od data -of important
```

### DONT Collect USN $J Journal

```text
CyLR --no-usnjrnl
```

## ORIGINAL AUTHORS

* [Jason Yegge](https://github.com/Lansatac)
* [Alan Orlikoski](https://github.com/rough007)
