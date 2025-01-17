# LionsBlog

A little blogging software written in Dotnet just because. :)

This software is meant to run your little blog, maybe on a RPi sitting on the shelf next to your router from home or one of those 10€ a year Low End VPSes, not for a high traffic site. At least i don't think that it will run so well in a high demand situation, a test is still pending.

One design goal was that the blog can be viewed and administrated from old browsers too, so no JS needed, this should even run from ye olde Navigator 4.5 or \*shouder\* IE6 running on Windows 95.

Another was being lean on traffic, at least here in Germany mobile traffic is prohibitly expensive and even when on a landline with DSL and unlimited traffic not every page has to be the size of the shareware installer of Doom. I am pretty happy that a call of the main site measures in kBytes and not in MBytes.

There is no non-anonymous tracking built-in, no need to get in touch with any DSGVO shenanigans and whats the point anyway. Less bookkeeping on the server too and i see that as a benefit.

If you want to edit the layout of the pages you shouldn't be afraid to dig trough the .cshtml files, but for most people editing wwwroot/default.css and Pages/_Layout.cshtml should be enough. The default is the layout i use for lionscade.de and is meant to be pretty laid back. As i said, lets all come back to website calls measuring in Kilobytes, not Megabytes.

This is using SQLite3 as its database and hence should run out of the box without any extra software.

Written using just VSCode on a little Debian box with love.

---
## **Installation**
### Quick and dirty for evaluation on your (Linux-)desktop, **not your server**:
* Check out on your desktop.
* Run `dotnet run`, point your browser at http://localhost:8080 and enjoy.

### Better:
This assumes that your installation uses systemd and Apache2, your desktop can compile Dotnet and that you have at least a general idea of managing a Linux server.
* Check out on your desktop.
* Run `dotnet publish --configuration Release` from the root of the project directory.
* Grab whats inside bin/Release/net9.0 and place somewhere you fancy that is reachable by a user that is not root.
* Check appsettings.json for general settings and Properties/launchSettings.json for ports and such, you might want to at least change TokenSecret in the appsettings.json.
* Do a test run if everything is in place, it should generate the images directory you specified and generate your database file. CTRL+C that when done.
* Disable RecoverAdminUser, IE. set it to false, in the appsettings.json, reenable if you ever forgot your password and need to recover it.
* Copy the kestrel-lionsblog.service file to /etc/systemd/system and edit it fitting to your configuration, you should need to touch WorkingDirectory, ExecStart and User.
* To start the service, run as root: `systemctl restart kestrel-lionsblog.service`
* Now we need to add this to Apache2 as a proxy service, copy 100-blog.conf to /etc/apache2/sites-available and edit the line ServerName accordingly.
* Run as root: `a2ensite 100-blog.conf`
* Point your browser to the domain you set up and enjoy.
* The next step would be setting up your certificates, i simply used certbot by Let's Encrypt, from my experience that can detect that you are using a proxy service just fine.

I will be more than happy to accept a pull request with an installation guide for nginx, lighttpd or even IIS on Windows. Or a better one for Apache2. ;)

---
## **TODO:**
* Uploading and handling of images in the control panel.
* Support for tags on the front-site.
* Dockerfile. Everyone loves Docker, or so i heard. ;)
* Unittests? At least some test suite that tries out several functions. For now i thoroughly tested everything manually but if this project gets bigger it's better to have some form of automated tests.

---
## **Future Ideas:**
* Gallery generation metatag
* Automatic thumbnail generation
* Optional WYSIWYG article editor, enabled trough the appsettings.json and would need modern JS. Would break one of the design goals tough.
* Markdown mode for the article editor, this could be handled server-side to keep with one of the design goals.

If you have any idea you would love to see in this little tool then feel free to add it to the issues as a suggestion. Or if you want to help out, make a pull request. <3

