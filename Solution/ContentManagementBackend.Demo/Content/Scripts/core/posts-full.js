var commentVM
var uloginStatus = 0 //0-no; 1-loading; 2-loaded

//ulogin
function loadUloginScript() {
    if (uloginStatus === 0) {
        uloginStatus = 1

        $.ajax({
            dataType: "script",
            cache: true,
            url: '//ulogin.ru/js/ulogin.js'
        }).done(function (script, textStatus) {
            uloginStatus = 2
        })
    }
}
function uLoginCallback(token) {
    commentVM.isLoading(true)
    ajaxRequest(
       '/Account/ULoginAjaxCallback',
       {
           token: token
       },
       function (data, hasError) {
           commentVM.isLoading(false)
           if (!hasError) {
               commentVM.userData(data)
               displayHeaderUser(data)
           }
       })
}
function displayHeaderUser(user) {
    if (user.userName && user.antiForgeryToken) {
        $('#loginActions').hide()

        var container = $('#logoutForm').show()
        container.find('span').text(user.userName)

        var hidden = $('<input name="__RequestVerificationToken" type="hidden">')
            .val(user.antiForgeryToken)
        container.prepend(hidden);
    }
}

$(function () {
    //comments
    function incrementCommentCount(increment) {
        var count = parseInt($('.com-totalcount').text())
        count += increment
        $('.com-totalcount').text(count)
        $('#commentTopCount').text(count)
    }
    function prepareComment(item) {
        var addTime = moment.utc(item.addTimeUtc).local()
        item.addTime = addTime.fromNow()

        if (item.children) {
            for (var c = 0; c < item.children.length; c++) {
                prepareComment(item.children[c])
            }
        }
    }
    
    commentVM = new Comments({
        initialRequest: parseInt($('.com-totalcount').text()) > 0,
        contentID: contentID,
        categoryID: categoryID,
        userData: userData,
        urls: {
            list: '/Comments/List',
            add: '/Comments/Add',
            remove: '/Comments/Delete',
            vote: '/Comments/Vote'
        },
        elements: {
            defaultInputID: 'commentsDefaultInput'
        },
        events: {
            onListResponse: function (data) {
                if (!data.list)
                    return [];

                for (var i = 0; i < data.list.length; i++) {
                    prepareComment(data.list[i])
                }

                return data.list;
            },
            onAddResponse: function (data) {
                prepareComment(data.comment)
                incrementCommentCount(1)
                return data.comment
            },
            onDeleteResponse: function (data) {
                incrementCommentCount(-1)

                var wrapper = $(data.event.target).closest('.com-wrapper')
                wrapper.find('[data-bind="value: controls.email"]').inputHint('', 'destroy')
                wrapper.find('[data-bind="value: controls.name"]').inputHint('', 'destroy')
            },
            onAuth: function (data) {
                displayHeaderUser(data.user)
            },
            onToggleReplyForm: function (data) {
                if (!data.item.isULoginVisible && uloginStatus === 2) {
                    data.item.isULoginVisible = true

                    var wrapper = $(data.event.target).closest('.com-wrapper')
                    var panel = wrapper.find('.com-social')

                    var id = 'social' + data.item.comment.commentID
                    var socialContainer = $('<div/>', {
                        'id': id,
                        'data-ulogin': 'display=panel;fields=first_name,last_name,photo,photo_big;providers=vkontakte,odnoklassniki,facebook,yandex,mailru,googleplus,twitter,youtube,google,livejournal,flickr,wargaming,instagram,vimeo,dudu,tumblr,liveid,foursquare,webmoney,soundcloud,uid,steam,linkedin,lastfm,openid;hidden=other;redirect_uri=;callback=uLoginCallback'
                    }).appendTo(panel);
                    uLogin.customInit(id)

                    bindInputHint(wrapper, data.item.controls)
                }
            }
        },
        inputAccessor: {
            findReplyInputId: function (comment, sender) {
                var input = $(sender).closest('.com-wrapper').find('.com-replyinput')
                return input.attr('id')
            },
            create: function (id, setFocus, initialText) {
                var inputPanel = $('#' + id).closest('.com-inputtable').find('.com-inputpanel')

                CKEDITOR.replace(id, {
                    resize_enabled: true,
                    toolbar: "addComment",
                    height: 80,
                    width: "100%",
                    on:
                    {
                        'instanceReady': function (ev) {
                            var editor = CKEDITOR.instances[id]

                            loadUloginScript()

                            if (setFocus) {
                                editor.focus()
                            }

                            if (initialText) {
                                editor.setData(initialText, {
                                    callback: function () {
                                        //cursor to end
                                        editor.focus() // Without this selection will be null on IE.
                                        var range = editor.createRange()
                                        range.moveToElementEditEnd(range.root)
                                        editor.getSelection().selectRanges([range])

                                        inputPanel.show()
                                    }
                                })
                            }
                            else {
                                CKEDITOR.instances[id].setData(defaultText)
                            }

                            CKEDITOR.instances[id].on('focus', function () {
                                inputPanel.show()

                                var actualText = CKEDITOR.instances[id].getData().trim()
                                if (actualText === defaultText) {
                                    CKEDITOR.instances[id].setData('')
                                }
                            });

                            CKEDITOR.instances[id].on('blur', function () {
                                var actualText = CKEDITOR.instances[id].getData().trim()
                                if (actualText === '') {
                                    CKEDITOR.instances[id].setData(defaultText)
                                }
                            });
                        }
                    },
                })
            },
            remove: function (id) {
                var editor = CKEDITOR.instances[id]
                if (editor) {
                    editor.destroy()
                }
            },
            getContent: function (id) {
                return CKEDITOR.instances[id].getData()
            },
            setContent: function (id, content) {
                CKEDITOR.instances[id].setData(content)
            }
        }
    })
    ko.applyBindings({
        comments: commentVM
    });

    //transform content
    var content = $(".af-content")
    content.find("a").attr("target", "_blank")
    content.find("img").each(function (ind, el) {
        var src = $(el).attr('src')
        $(el).wrap('<a href="' + src + '" rel="pretty[all]"></a>')
    })
    content.find("a[rel^='pretty']").prettyPhoto({
        show_title: false,
        social_tools: false
    })
       
    //inputHint
    function bindInputHint(contentWrapper, controlsVM) {
        var emailInput = contentWrapper.find('[data-bind="value: controls.email"]').inputHint(emailText)
        var nameInput = contentWrapper.find('[data-bind="value: controls.name"]').inputHint(nameText)

        var defaultClear = controlsVM.clear
        controlsVM.clear = function () {
            defaultClear()

            //trigger input hint
            emailInput.val('').trigger('change')
            nameInput.val('').trigger('change')
        }
    }
    var defaultInputWrapper = $('#commentsDefaultInput').closest('.com-inputtable')
    bindInputHint(defaultInputWrapper, commentVM.controls)
});