/**
 * @license Copyright (c) 2003-2015, CKSource - Frederico Knabben. All rights reserved.
 * For licensing, see LICENSE.md or http://ckeditor.com/license
 */

CKEDITOR.editorConfig = function( config ) {
    config.scayt_autoStartup = false;
    config.disableNativeSpellChecker = false;
    config.extraPlugins = 'youtube,uploadwidget,autogrow,doksoft_easy_image';
    config.allowedContent = true;
    config.removePlugins = 'elementspath,scayt,resize';
    config.defaultLanguage = 'ru';
    config.language = 'ru';
    config.resize_enabled = true;
    config.toolbarCanCollapse = false;
    config.startupShowBorders = false;
    config.wordcount = {
        showParagraphs: false,
        showWordCount: false,
        showCharCount: false,
        charLimit: -1,
        wordLimit: -1
    };
    config.tabSpaces = 5;
    //config.font_defaultLabel = 'Tahoma';
    //config.fontSize_defaultLabel = '14px';
    //config.extraCss += "body{font:14px Tahoma,Arial,sans-serif,Helvetica,Arial,Verdana;}";

    //autogrow
    config.autoGrow_minHeight = 80;

    //upload image
    config.uploadUrl = '/Posts/UploadContentImage';

    //doksoft_easy_image
    config.doksoft_uploader_url = '/Posts/UploadContentImage';

    // Make dialogs simpler
    config.removeDialogTabs = 'image:advanced;image:Link;link:advanced;link:upload';
    config.linkShowTargetTab = false;

    config.toolbar_addNews =
    [
        ['Undo', 'Redo', 'Cut', 'Copy', 'Paste', 'PasteText', 'PasteFromWord'],
        ['Bold', 'Italic', 'Underline', 'Strike', '-', 'TextColor', 'BGColor'],
        ['JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock'],
        '/',
        ['NumberedList', 'BulletedList', '-', 'Outdent', 'Indent', 'Blockquote'],
        ['Styles'],
        ['Link', 'doksoft_easy_image', 'Youtube', 'Table', 'HorizontalRule', 'Smiley']
    ];

    config.toolbar_addComment =
    [
        ['Undo', 'Redo', 'Bold', 'Italic', 'Underline', 'Strike', 'Smiley'],
    ]

    config.smiley_descriptions = ["улыбаюсь", "грущу", "подмигиваю", "смеюсь", "хмурюсь", "дерзю", "краснею",
        "удивляюсь", "в нерешительности", "злюсь", "ангел", "крутой", "дьявол", "плачу", "эврика", "нет", "да", "сердце", "сердце разбито", "поцелуй", "почта"];

};
