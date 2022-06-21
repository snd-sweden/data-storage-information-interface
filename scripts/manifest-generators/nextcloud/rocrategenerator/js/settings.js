 jQuery(function() {

	jQuery('#saveManifestInFolder').on('change', function() {
		var status = 'no';
		if (jQuery(this).is(':checked')) {
			status = 'yes';
		}
		OCP.AppConfig.setValue('rocrategenerator', 'saveManifestInFolder', status);
	});

	jQuery('#postManifestToUrl').on('input', function() {
    var value = jQuery(this).val() ?? "";

		OCP.AppConfig.setValue('rocrategenerator', 'postManifestToUrl', value);
	});

	jQuery('#publisherDomain').on('input', function() {
    var value = jQuery(this).val() ?? "";

		OCP.AppConfig.setValue('rocrategenerator', 'publisherDomain', value);
	});

});
