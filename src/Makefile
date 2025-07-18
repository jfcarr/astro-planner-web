publish_path=AstroPlanner/bin/Release/net8.0/publish/wwwroot/
serve_path=output
ssh_username=$(DH_USER)
ssh_password=$(DH_PASSWORD)
ssh_port=$(DH_PORT)
ssh_host=$(DH_HOST)
remote_path=$(DH_ROOT)/www/astroplan.webwhimsy.com  # release

default:
	@echo 'Targets:'
	@echo '  watch      -- Run project and watch for code changes.'
	@echo '  publish    -- Generate website from project.'
	@echo '  php-serve  -- Serve generated website locally using PHP dev server.'
	@echo '  deploy     -- Copy generated website to web host.'
	@echo '  clean      -- Remove binaries and other generated files.'

watch:
	cd AstroPlanner; dotnet watch

publish:
	cd AstroPlanner; dotnet publish -c Release
	rm -rf $(serve_path)
	cp -r $(publish_path) $(serve_path)
	cd $(serve_path); sed -i 's|<base href="/" />|<base href="https://github.com/jfcarr/astro-planner-web/" />|' index.html

php-serve:
	@cd $(serve_path); php -S 0.0.0.0:8088

deploy-disabled:
	@sshpass -p '$(ssh_password)' rsync -avze 'ssh -p $(ssh_port)' $(publish_path) $(ssh_username)@$(ssh_host):$(remote_path)

clean:
	-rm -rf AstroPlanner/bin/
	-rm -rf AstroPlanner/obj/
	-rm -rf AstroPlanner.Util/bin/
	-rm -rf AstroPlanner.Util/obj/
