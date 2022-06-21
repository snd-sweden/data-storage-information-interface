# RO-Crate proof of concept plugin for nextcloud

This example is not a complete or anywhere near a production ready plugin for [nextcloud](https://nextcloud.com).

Main purpose of this plugin is to provide a proof of concept and example of how to generate a RO-Crate manifest.

This code could be used as a base for a customized plugin.

# Test using docker-compose
1. Install docker and docker-compose https://www.docker.com
2. Run `docker-compose up` in this directory
3. Visit http://localhost:8080
4. Create new admin account
5. Go to http://localhost:8080/settings/apps/disabled and enable the "Ro Crate Generator (Proof of concept)" app (untrusted)
6. Go to http://localhost:8080/settings/admin/sharing#roCrateSettings fill in your domain in "Publisher domain"
7. "Create RO-Crate" should now be availible when right-clicking on a folder in the file explorer inside nextcloud

# Testing whitout docker
Place the folder `rocrategenerator` in  **nextcloud/apps/**
