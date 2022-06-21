jQuery(document).ready(function () {

  OCA.Files.fileActions.registerAction({
    name: 'rocrate',
    displayName: "Create RO-Crate",
    mime: 'dir',
    permissions: OC.PERMISSION_UPDATE,
    icon: '',

    actionHandler: function (filename, context) {
      var tr = context.fileList.findFileEl(filename);
      context.fileList.showFileBusyState(tr, true);
      var data = {
        path: context.dir + "/" + filename,
      };

      jQuery.ajax({
        type: "POST",
        async: "false",
        url: OC.generateUrl('apps/rocrategenerator'),
        data: data,
        success: function (response) {
          context.fileList.showFileBusyState(tr, false);
        },
        error: function (response) {
          OC.Notification.showTemporary('Unable to create RO-Crate manifest.');
          context.fileList.showFileBusyState(tr, false);
        }
      });
    }

  });

});