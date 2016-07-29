
(function () {
    var inputAdded = false
    var loadingImage = 'data:image/gif;base64,R0lGODlhDgAOAIAAAAAAAP///yH5BAAAAAAALAAAAAAOAA4AAAIMhI+py+0Po5y02qsKADs='
    var pluginName = 'dumbimageupload'


    CKEDITOR.plugins.add(pluginName, {
        icons: 'dumbimageupload',
        requires: 'uploadwidget',
        init: function (editor) {           
            var fileTools = CKEDITOR.fileTools,
                uploadUrl = fileTools.getUploadUrl(editor.config, 'image')
                fileInputId = pluginName + 'input'

            if (!uploadUrl) {
                CKEDITOR.error('uploadimage-config')
                return
            }


            //ckeditor file tools
            var uploadWidget = {
                supportedTypes: /image\/(jpg|jpeg|png|gif|bmp)/,
                uploadUrl: uploadUrl,

                parts: {
                    img: 'img'
                },

                //initEvents: function(){
                //    var uploads = editor.uploadRepository;
                //    var capitalize = CKEDITOR.tools.capitalize;
                //    var loader = uploads.create(file);
                //    var id = loader.id;
                //    var widget = this;

                //    loader.on('update', function (evt) {
                //        editor.fire('lockSnapshot');

                //        // Call users method, eg. if the status is `uploaded` then
                //        // `onUploaded` method will be called, if exists.
                //        var methodName = 'on' + capitalize(loader.status);

                //        if (typeof widget[methodName] === 'function') {
                //            if (widget[methodName](loader) === false) {
                //                editor.fire('unlockSnapshot');
                //                return;
                //            }
                //        }

                //        // Remove widget on error or abort.
                //        if (loader.status == 'error' || loader.status == 'abort') {
                //            editor.widgets.del(widget);
                //        }

                //        editor.fire('unlockSnapshot');
                //    })

                //    loader.update();
                //},

                fileToElement: function () {
                    var img = new CKEDITOR.dom.element('img');                   
                    img.setAttribute('src', loadingImage);
                    img.setAttribute('style', 'opacity:0.3;width:100%;');
                    this.parts.img = img;

                    return img;
                },

                onUploading: function (upload) {
                    // Show the image during the upload.
                    this.parts.img.setAttribute('src', upload.data);
                },

                onUploaded: function (upload) {
                    var img = this.parts.img;

                    // Set width and height to prevent blinking.
                    var newImg = '<img src="' + upload.url + '" ' +
                        'width="' + img.$.naturalWidth + '" ' +
                        'height="' + img.$.naturalHeight + '">'

                    this.replaceWith(newImg, 'html')
                },

                //replaceWith: function (data, mode) {
                //    if (data.trim() === '') {
                //        editor.widgets.del(this);
                //        return;
                //    }

                //    var wasSelected = (this == editor.widgets.focused),
                //        editable = editor.editable(),
                //        range = editor.createRange(),
                //        bookmark, bookmarks;

                //    if (!wasSelected) {
                //        bookmarks = editor.getSelection().createBookmarks();
                //    }

                //    var wrapper = this.parts.img;
                //    range.setStart(wrapper);
                //    range.setEndAfter(wrapper);

                //    if (wasSelected) {
                //        bookmark = range.createBookmark();
                //    }

                //    editable.insertHtmlIntoRange(data, range, mode);

                //    editor.widgets.checkWidgets({ initOnlyNew: true });

                //    // Ensure that old widgets instance will be removed.
                //    // If replaceWith is called in init, because of paste then checkWidgets will not remove it.
                //    editor.widgets.destroy(pluginName);

                //    if (wasSelected) {
                //        range.moveToBookmark(bookmark);
                //        range.select();
                //    } else {
                //        editor.getSelection().selectBookmarks(bookmarks);
                //    }

                //}
            }
            

            //file input
            var createFileInput = function () {
                if (inputAdded)
                    return

                $('<input>', {
                    id: fileInputId,
                    type: 'file'
                })
                .css({ "visibility": "hidden" })
                .on('change', handleFileChange)
                .appendTo('body')
                
                inputAdded = true
            }
            
            var handleFileChange = function (event) {
                var files = event.target.files,
                    def = uploadWidget,
                    loadMethod = def.loadMethod || 'loadAndUpload'

                if (!files || !files.length)
                    return
              
                for (i = 0; i < files.length; i++) {
                    var file = files[i]

                    if (!def.supportedTypes || fileTools.isTypeSupported(file, def.supportedTypes)) {
                        var ckElement = def.fileToElement(file)
                        editor.insertElement(ckElement)
                        
                        var loader = editor.uploadRepository.create(file)
                        loader.upload(uploadUrl)

                        fileTools.markElement(ckElement, pluginName, loader.id)
                        fileTools.bindNotifications(editor, loader)
                        
                        var widget = def

                        loader.on('update', function (evt) {
                            editor.fire('lockSnapshot');

                            // Call users method, eg. if the status is `uploaded` then
                            // `onUploaded` method will be called, if exists.
                            var methodName = 'on' + CKEDITOR.tools.capitalize(loader.status);

                            if (typeof widget[methodName] === 'function') {
                                if (widget[methodName](loader) === false) {
                                    editor.fire('unlockSnapshot');
                                    return;
                                }
                            }

                            // Remove widget on error or abort.
                            if (loader.status == 'error' || loader.status == 'abort') {
                                editor.widgets.del(widget);
                            }

                            editor.fire('unlockSnapshot');
                        })

                    }
                }


            }
            

            //init
            createFileInput()
            
            //editor.widgets.add(pluginName, uploadWidget)
            fileTools.addUploadWidget(editor, pluginName, uploadWidget)
            
            editor.ui.addButton(pluginName, {
                label: editor.lang.common.image,
                command: pluginName,
                toolbar: 'insert,10'
            })
            editor.addCommand(pluginName, new CKEDITOR.command(editor, {
                exec: function (editor) {
                    $('#' + fileInputId).click()
                }
            }))
        }
    })

})();
