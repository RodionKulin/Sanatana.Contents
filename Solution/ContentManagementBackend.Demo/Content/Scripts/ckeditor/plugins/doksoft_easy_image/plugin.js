(function () {
    var pluginName = "doksoft_easy_image"

    
    //config
    function setImageConfig(L, M) {
        CKEDITOR.config["doksoft_easy_image_" + L] = M;
    }

    function getImageConfig(M, L) {
        return M.config["doksoft_easy_image_" + L];
    }

    function readConfig(M, L) {
        return M.config[L];
    }
            
    function getEditorVersion() {
        return CKEDITOR.version.charAt(0) == "3" ? 3 : 4;
    }

    function getLabelText(editor) {
        var key = "doksoft_easy_image_button_label"

        if (getEditorVersion() == 3) {
            if (typeof (editor.lang[key]) !== "undefined") {
                return editor.lang[key];
            } else {
                console.log("(v3) editor.lang['doksoft_easy_image'] not defined");
            }
        } else {
            if (typeof (editor.lang[pluginName]) !== "undefined") {
                if (typeof (editor.lang[pluginName][key]) !== "undefined") {
                    return editor.lang[pluginName][key];
                } else {
                    console.log("editor.lang['doksoft_easy_image']['" + key + "'] not defined");
                }
            } else {
                console.log("editor.lang['doksoft_easy_image'] not defined");
            }
        }
        return "";
    }

    var o = 101;
    CKEDITOR.config["doksoft_uploader_url"] = CKEDITOR.plugins.getPath("doksoft_uploader") + "uploader.php";

    setImageConfig("wrap_min_count", 1);
    setImageConfig("wrap_template", "");
    if (o == 101) {
        setImageConfig("accept_types", "image/*");
        setImageConfig("template", "<img src='{IMAGE}' />");
        //setImageConfig("template", "<div class='cke-cenimage'>"
        //    + "<img src='{IMAGE}' />"
        //    + "</div>");
    }
    else if (o == 102) {
        setImageConfig("accept_types", "image/*");
        setImageConfig("template", "<a class='doksoft_preview_href' rel='lightbox' href='{IMAGE}'><img src='{PREVIEW}'/></a>");
    }
    else if (o == 103) {
        setImageConfig("accept_types", "");
        setImageConfig("template",
            "<div style='display:inline-block'>"
            + '<img src="{PLUGIN_PATH}img/download.png" style="width:24px;height:24px;margin-right:5px;margin-top:-4px;vertical-align: middle"/>'
            + "<a class='doksoft_easy_file' style=\"font-size:16px;margin-right:15px;\" href='{FILE}'>Download file</a>"
            + "</div>");
    }

    if (o == 101 || o == 102) {
        setImageConfig("img_width", 0);
        setImageConfig("img_height", 0);
        setImageConfig("img_enlarge", false);
    }
    if (o == 102) {
        setImageConfig("thumb_width", 320);
        setImageConfig("thumb_height", 240);
        setImageConfig("thumb_enlarge", true);
    }
    

    //display
    function findIcon(L) {
        var N = L.container.$;
        if (N == null) {
            return null;
        }
        var icons = N.getElementsByClassName("cke_button__doksoft_easy_image_icon");
        if (icons.length > 0) {
            return icons[0];
        } else {
            return null;
        }
    }

    function showLoading(L) {
        var icon = findIcon(L);
        if (icon != null) {
            if (icon.tagName == "IMG") {
                icon.src = icon.src.replace(/(mce_)?icons\/doksoft_easy_image(_3)?(_4\.0)?\.png/, "img/progress.gif");
            } else {
                icon.style.backgroundImage = icon.style.backgroundImage.replace(/(mce_)?icons\/doksoft_easy_image(_3)?(_4\.0)?\.png/, "img/progress.gif");
            }
        }
    }

    function hideLoading(L) {
        var icon = findIcon(L);
        if (icon != null) {
            if (icon.tagName == "IMG") {
                icon.src = icon.src.replace(/img\/progress\.gif/, "icons/doksoft_easy_image" + ".png");
            } else {
                icon.style.backgroundImage = icon.style.backgroundImage.replace(/img\/progress\.gif/, "icons/doksoft_easy_image" + ".png");
            }
        }
    }

    function renderResult(editor, results) {
        var errors = [];
        var urls = [];
        for (var i = 0; i < results.length; i++) {
            var resultObj = JSON.parse(results[i])
            if (resultObj.uploaded == 1) {
                urls.push(resultObj.url);
            }
            else if (resultObj.uploaded == 0
                && resultObj.error && resultObj.error.message) {
                errors.push(resultObj.error.message);
            }
            else {
                errors.push(connectionException);
            }
        }
        if (urls.length > 0) {
            for (var i = 0; i < urls.length; i++) {
                var html = fromTemplate(editor, urls[i]);
                insertHtml(editor, html);
            }
        }
        if (errors.length > 0) {
            for (var i = errors.length - 1; i >= 1; i--) {
                var isUnique = true;
                for (var j = errors.length - 2; j >= 0; j--) {
                    if (errors[j] == errors[i]) {
                        isUnique = false;
                    }
                }
                if (!isUnique) {
                    delete errors[i];
                }
            }
            if (errors.length == 1) {
                logError(errors[0]);
            } else {
                for (var i = 0; i < errors.length; i++) {
                    logError(errors[i]);
                }
            }
        }
    }
    

    //dom
    function insertHtml(editor, html) {
        var el = CKEDITOR.dom.element.createFromHtml(html);
        editor.insertElement(el);
    }

    function fromTemplate(P, M) {
        var L = M.substring(M.lastIndexOf("/") + 1);
        var O = getImageConfig(P, "template");

        if (o == 101) {
            O = O.replace(/\{IMAGE\}/g, M);
            O = O.replace(/\{FILENAME\}/g, L);
            return O;
        } else if (o == 102) {
            O = O.replace(/\{IMAGE\}/g, M);
            O = O.replace(/\{FILENAME\}/g, L);
            var N = M.split(".");
            N[N.length - 2] += "_small";
            M = N.join(".");
            L = M.substring(M.lastIndexOf("/") + 1);
            O = O.replace(/\{PREVIEW\}/g, M);
            O = O.replace(/\{FILENAME\}/g, L);
            return O;
        } else if (o == 103) {
            O = O.replace(/\{FILE\}/g, M);
            O = O.replace(/\{PLUGIN_PATH\}/g, CKEDITOR.plugins.getPath(pluginName));
            O = O.replace(/\{FILENAME\}/g, L);
            return O;
        }
    }
    
    
    //command
    function createFileInput(editor) {
        var imageForm = document.getElementById("doksoft_easy_imageForm");
        if (imageForm != null) {
            imageForm.parentNode.removeChild(imageForm);
        }
        imageForm = document.createElement("div");
        imageForm.id = "doksoft_easy_imageForm";
        imageForm.setAttribute("style", "display:none");
        var acceptTypes = "";
        if (getImageConfig(editor, "accept_types").trim().length > 0) {
            acceptTypes = ' accept="' + getImageConfig(editor, "accept_types") + '"';
        }
        imageForm.innerHTML = '<form enctype="multipart/form-data">'
            + '<input type="file" name="file" multiple="multiple" id="doksoft_easy_imageFile"' + acceptTypes + ">"
            + '<input type="submit" id="doksoft_easy_imageSubmit">'
            + "</form>";
        document.getElementsByTagName("body")[0].appendChild(imageForm);

    }

    function applyCommand(editor) {
        createFileInput(editor)
        var fileInput = document.getElementById("doksoft_easy_imageFile");

        fileInput.addEventListener("change", (function () {
            var url = getUrl(editor);

            return function() {
                showLoading(editor);
                var filesCount = fileInput.files.length;
                var results = new Array(filesCount);
                var filesProcessed = 0;
                for (var i = 0; i < filesCount; i++) {
                    var data = new FormData();
                    data.append("file", fileInput.files[i]);
                    sendRequest(i, editor, url, data
                        , function (fIndex, editor, response) {
                            results[fIndex] = response;
                            filesProcessed++;
                            if (filesProcessed == filesCount) {
                                hideLoading(editor);
                                renderResult(editor, results);
                            }
                        }, function(fIndex, editor, response) {
                            results[fIndex] = response;
                            filesProcessed++;
                            if (filesProcessed == filesCount) {
                                hideLoading(editor);
                                renderResult(editor, results);
                            }
                        });
                }

            };
        })());

        fileInput.click();
    }

    function getUrl(editor) {
        var url = editor.config["doksoft_uploader_url"];
        if (url.indexOf("?") >= 0) {
            url += "&";
        } else {
            url += "?";
        }
        url += "client=plupload&";
        if (o == 101) {
            url += "type=Images";
        } else {
            if (o == 102) {
                url += "type=Images&makeThumb=true";
            } else {
                if (o == 103) {
                    url += "type=Files";
                }
            }
        }
        if (o == 101 || o == 102) {
            var N = getImageConfig(editor, "img_width");
            var L = getImageConfig(editor, "img_height");
            var S = getImageConfig(editor, "img_enlarge");
            if (N > 0) {
                url += "&iw=" + N;
            }
            if (L > 0) {
                url += "&ih=" + L;
            }
            if (S) {
                url += "&ie=1";
            }
        }
        if (o == 102) {
            var O = getImageConfig(editor, "thumb_width");
            var Q = getImageConfig(editor, "thumb_height");
            var R = getImageConfig(editor, "thumb_enlarge");
            if (O > 0) {
                url += "&tw=" + O;
            }
            if (Q > 0) {
                url += "&th=" + Q;
            }
            if (R) {
                url += "&te=1";
            }
        }
        return url;
    }
        
    function sendRequest(fIndex, editor, url, data, successHandler, failHandler) {
        var request;
        if (window.XMLHttpRequest) {
            request = new XMLHttpRequest();
        } else {
            request = new ActiveXObject("Microsoft.XMLHTTP");
        }
        request.onreadystatechange = function () {
            if (request.readyState == 4) {
                if (request.status == 200) {
                    successHandler(fIndex, editor, request.responseText);
                } else {
                    failHandler(fIndex, editor, request.responseText);
                }
            }
        };
        request.open("POST", url, true);
        request.send(data);
    }
    

    //plugin
    CKEDITOR.plugins.add(pluginName, {
        lang: ["ru"],
        init: function(editor) {
            editor.addCommand(pluginName, {
                exec: function (editor) {
                    applyCommand(editor);
                }
            });
            editor.ui.addButton(pluginName, {
                label: getLabelText(editor),
                icon: this.path + "icons/doksoft_easy_image.png",
                command: pluginName
            });
        }
    });
})();