$(function () {
    //functions
    var initImageUploader = function (settings) {
        //functions
        var bindDrag = function () {
            $(document).bind('dragover', function (e) {
                e.preventDefault()
            }).bind('drop', function (e) {
                settings.elements.dropZone.removeClass("upl-dropzone-hover")
                e.preventDefault()
            });

            settings.elements.dropZone.on('dragenter', function () {
                $(this).addClass("upl-dropzone-hover")
            })
            .on('dragleave', function () {
                $(this).removeClass("upl-dropzone-hover")
            })
            .bind('drop', function () { })
        }

        var initVisibility = function () {
            var status = settings.elements.imageStatusHidden.val()

            if (status !== 'NotSet') {
                settings.elements.deleteButton.show()
            }

            if (status !== 'Static' && getOriginalSrc()) {
                settings.elements.restoreButton.show()
            }
        }

        var getOriginalSrc = function () {
            return settings.elements.originalUrlHidden.val()
        }

        var setSrc = function (url, status, fileId) {
            if (url) {
                settings.elements.imageEmpty.hide()
                settings.elements.image.attr("src", url).show()
            }
            else {
                settings.elements.imageEmpty.show()
                settings.elements.image.hide()
            }

            settings.elements.imageLoading.hide()
            settings.elements.imageStatusHidden.val(status)
            settings.elements.imageIDHidden.val(fileId)

            //deleteButton
            if (status === 'NotSet')
                settings.elements.deleteButton.hide()
            else
                settings.elements.deleteButton.show()

            //restoreButton
            if (getOriginalSrc() && status !== 'Static')
                settings.elements.restoreButton.show()
            else
                settings.elements.restoreButton.hide()
        }

        var showLoading = function () {
            settings.elements.image.hide()
            settings.elements.imageEmpty.hide()
            settings.elements.imageLoading.show()
        }

        var deleteImage = function (e) {
            e.preventDefault()

            setSrc(null, 'NotSet', null)
        }

        var restoreImage = function (e) {
            e.preventDefault()

            var original = getOriginalSrc()
            setSrc(original, 'Static', null)
        }

        var downloadFromUrl = function (e) {
            e.preventDefault()

            ajaxRequest(
               settings.urls.downloadUrl,
               {
                   url: settings.elements.urlInput.val()
               },
               function (data, hasError) {
                   if (hasError)
                       return;

                   setSrc(data.url, 'Temp', data.fileID)
               })
        }

        //binding
        bindDrag()
        initVisibility()

        settings.elements.deleteButton.on('click', deleteImage)
        settings.elements.restoreButton.on('click', restoreImage)
        settings.elements.downloadUrlButton.on('click', downloadFromUrl)

        settings.elements.inputUploader.fileupload({
            url: settings.urls.uploadUrl,
            dataType: 'json',
            autoUpload: true,
            dropZone: settings.elements.dropZone,
            add: function (e, data) {
                var acceptFileTypes = /^image\/(gif|jpe?g|png)$/i;
                if (!acceptFileTypes.test(data.originalFiles[0]['type'])) {
                    logError(settings.messages.fileTypeError);
                    return;
                }
                if (data.originalFiles[0]['size'] > settings.maxFileSize) {
                    logError(settings.messages.maxSizeError);
                    return;
                }

                showLoading()
                data.submit()
            },
            done: function (e, data) {
                if (!data.result) {
                    setSrc(null, 'NotSet', null)
                    logError(settings.messages.uploadFailError)
                    return
                }
                if (data.result.error) {
                    setSrc(null, 'NotSet', null)
                    logError(data.result.error)
                    return
                }

                setSrc(data.result.url, 'Temp', data.result.fileID)
            },
            fail: function (e, data) {
                setSrc(null, 'NotSet', null)
                logError(settings.messages.uploadFailError);
            }
        })
    }

    var initDatetimePicker = function (settings) {
        var utcTime = settings.hidden.val()
        var localTime = time_ToLocal(utcTime, momentFormat)

        var input = settings.container.find('input')
        input.val(localTime)

        settings.container.datetimepicker({
            locale: 'ru',
            format: momentFormat,
            sideBySide: true,
            keepOpen: true,
            useCurrent: true,
            icons: {
                time: 'fa fa-clock-o',
                date: 'fa fa-calendar',
                up: 'fa fa-chevron-up',
                down: 'fa fa-chevron-down',
                previous: 'fa fa-chevron-left',
                next: 'fa fa-chevron-right'
            },
            tooltips: settings.tooltips
        }).on('dp.change', function (e) {
            if (e.date) {
                var iso = e.date.toISOString()
                settings.hidden.val(iso)
            }
            else {
                settings.hidden.val('')
            }
        });
    }

    var initDeletePost = function (settings) {       
        settings.deleteButton.confirmation({
            title: settings.messages.title,
            btnOkLabel: settings.messages.btnOkLabel,
            btnCancelLabel: settings.messages.btnCancelLabel,
            popout: true,
            onConfirm: function (e) {
                e.preventDefault()
                settings.loadingElement.show()
                settings.controls.prop("disabled", true);

                ajaxRequest(
                   settings.deleteUrl,
                   {
                       contentID: settings.contentID
                   },
                   function (data, hasError) {
                       settings.loadingElement.hide()
                       settings.controls.prop("disabled", false);

                       if (hasError)
                           return;

                       window.location.href = settings.onCompleteUrl;
                   })
            }
        })
    }

    var showUpdateNonceMessage = function (settings) {      
        
        settings.saveButton.on('click', function () {
            settings.useNonceHidden.val('False')
            settings.submitForm.submit();
        })

        if (settings.modalContainer.length > 0) {
            settings.modalContainer.modal()
        }
    }

    var initContentPreview = function (settings) {

        settings.showButton.on('click', function (e) {
            e.preventDefault()

            var isVisible = settings.container.is(":visible")

            if (isVisible) {
                settings.container.hide()
            }
            else {
                var data = CKEDITOR.instances[settings.editorID].getData()
                settings.container.show()
                settings.textPlaceholder.html(data)
            }            
        })
    }


    //init
    CKEDITOR.replace('shortContent', {
        "height": "150",
        "htmlEncodeOutput": true,
        "toolbar": "addNews",
        "width": "100%"
    })

    CKEDITOR.replace('fullContent', {
        "height": "150",
        "htmlEncodeOutput": true,
        "toolbar": "addNews",
        "width": "100%"
    })

    initImageUploader({
        maxFileSize: maxFileSize,
        urls: {
            uploadUrl: '/Posts/UploadPreviewImage',
            downloadUrl: '/Posts/DownloadPreviewImageFromUrl'
        },
        elements: {
            image: $('.upl-img'),
            imageLoading: $('.upl-loading'),
            imageEmpty: $('.upl-empty'),
            inputUploader: $('#uplFileupload'),
            dropZone: $(".upl-dropzone"),
            urlInput: $('.upl-urlinput'),
            downloadUrlButton: $('.url-urlbutton'),
            deleteButton: $(".upl-dropzone").find('.upl-delete'),
            restoreButton: $('#uplRestore'),
            imageStatusHidden: $('#imageStatus'),
            imageIDHidden: $('#imageID'),
            originalUrlHidden: $('#originalUrl')
        },
        messages: {
            maxSizeError: maxSizeError,
            fileTypeError: fileTypeError,
            uploadFailError: uploadFailError
        }
    })

    initDatetimePicker({
        hidden: $('#publishTimeUtc'),
        container: $('#datetimepicker1'),
        tooltips: datepickerLocals
    })

    initDeletePost({
        deleteUrl: '/Posts/Delete',
        onCompleteUrl: '/Posts/Delete',
        deleteButton: $('#deletePost'),
        loadingElement: $('#panelLoading'),
        controls: $('.art-actionpanel').find('input'),
        contentID: $('#contentID').val(),
        messages: confirmLocals
    })
    
    showUpdateNonceMessage({
        modalContainer: $('#nonceModal'),
        useNonceHidden: $('#matchUpdateNonce'),
        submitForm: $('#articleForm'),
        saveButton: $('#ignoreNonce')
    })

    initContentPreview({
        showButton: $('#fullPreviewTrigger'),
        container: $('#fullPreviewContainer'),
        textPlaceholder: $('#fullPreviewText'),
        editorID: 'fullContent'
    })
    
    
});
