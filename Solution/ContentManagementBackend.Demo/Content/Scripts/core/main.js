var momentZoneFormat = 'DD.MM.YYYY HH:mm (Z)',
    momentFormat = 'DD.MM.YYYY HH:mm'
    momentDateFormat = 'DD.MM.YYYY',
    momentLocale = 'ru',
    defaultSearchText = "поиск"

/*init*/
$(function () {   
    //tooltips
    $('.bs-tooltip').tooltip({
        html: true,
        container: 'body'
    })
            
    //time
    moment.locale(momentLocale);
    time_ToLocals($('.to-local-datetime'), momentFormat)
    time_ToLocals($('.to-local-date'), momentDateFormat)

    //menu
    createMenu()
    
    //search
    var searchInput = $("#search")
    $(".search a").on('click', function (e) {       
        e.preventDefault()
        if (!searchInput.hasClass('empty')) {
            search(searchInput.val())
        }
    })
    searchInput.keypress(function (e) {
        if (e.keyCode === 13 && !searchInput.hasClass('empty')) {
            e.preventDefault()
            search(searchInput.val())
        }
    })       
    searchInput.inputHint(defaultSearchText)
});

//menu
function createMenu() {
    $(".menu-roots>li>a").on('click', function (e) {
        var sub = $(this).closest("li").find(".menu-drop")
        var isParent = $(this).attr("href") === "#"
        var isActive = $(this).hasClass('active')
        var isOpened = sub.is(":visible")

        if (isParent) {
            if (!isActive) {
                if (isOpened)
                    sub.hide()
                else
                    sub.show()
            }

            e.preventDefault()
        }
    })

    var isTouchEvent = false
    $(".menu-roots>li").on('touchstart', function () {
        isTouchEvent = true
    })
    .on('mouseenter', function () {
        //disable mouseenter on touch device
        if (isTouchEvent) {
            isTouchEvent = false
            return
        }

        var isActive = $(this).children('a').hasClass('active')
        if (!isActive) {
            $(this).find(".menu-drop").show()
        }
    })
    .on('mouseleave', function () {
        $(this).find(".menu-drop").hide()
    })
}

//jQuery Caret + inputHint
(function ($) {
    $.fn.caret = function (pos) {
        var target = this[0];
        var isContentEditable = target.contentEditable === 'true';
        //get
        if (arguments.length == 0) {
            //HTML5
            if (window.getSelection) {
                //contenteditable
                if (isContentEditable) {
                    target.focus();
                    var range1 = window.getSelection().getRangeAt(0),
                        range2 = range1.cloneRange();
                    range2.selectNodeContents(target);
                    range2.setEnd(range1.endContainer, range1.endOffset);
                    return range2.toString().length;
                }
                //textarea
                return target.selectionStart;
            }
            //IE<9
            if (document.selection) {
                target.focus();
                //contenteditable
                if (isContentEditable) {
                    var range1 = document.selection.createRange(),
                        range2 = document.body.createTextRange();
                    range2.moveToElementText(target);
                    range2.setEndPoint('EndToEnd', range1);
                    return range2.text.length;
                }
                //textarea
                var pos = 0,
                    range = target.createTextRange(),
                    range2 = document.selection.createRange().duplicate(),
                    bookmark = range2.getBookmark();
                range.moveToBookmark(bookmark);
                while (range.moveStart('character', -1) !== 0) pos++;
                return pos;
            }
            // Addition for jsdom support
            if (target.selectionStart)
                return target.selectionStart;
            //not supported
            return 0;
        }
        //set
        if (pos == -1)
            pos = this[isContentEditable ? 'text' : 'val']().length;
        //HTML5
        if (window.getSelection) {
            //contenteditable
            if (isContentEditable) {
                target.focus();
                window.getSelection().collapse(target.firstChild, pos);
            }
                //textarea
            else
                target.setSelectionRange(pos, pos);
        }
            //IE<9
        else if (document.body.createTextRange) {
            if (isContentEditable) {
                var range = document.body.createTextRange();
                range.moveToElementText(target);
                range.moveStart('character', pos);
                range.collapse(true);
                range.select();
            } else {
                var range = target.createTextRange();
                range.move('character', pos);
                range.select();
            }
        }
        if (!isContentEditable)
            target.focus();
        return this;
    }
})(jQuery);
(function ($) {
    function init(input, defaultText) {
        var isComboStarted = false

        function isDefault() {
            var isDef = input.hasClass('empty')
            var value = input.val()
            return isDef && value !== ''
        }
        function removeDefault() {         
            if (isDefault()) {
                input.removeClass('empty').val('')              
            }
        }
        function setDefault() {
            var isEmpty = input.val() === ""
            if (isEmpty) {
                input.val(defaultText).addClass('empty').caret(0)
            }
        }

        input.val(defaultText).addClass('empty')
            .on('focus', function (e) {
                if (isDefault()) {
                    setTimeout(function () {
                        if (input.is(':focus')) {
                            input.caret(0)
                        }
                    }, 10)
                }
            })
            .on('touchstart', function () {
                removeDefault()
            })
            .on('keydown', function (e) {
                var isDef = isDefault()
                var isChar = e.which == 32 || // spacebar
                    (e.which > 47 && e.which < 58) || // number keys
                    (e.which > 64 && e.which < 91) || // letter keys
                    (e.which > 95 && e.which < 112) || // numpad keys
                    (e.which > 185 && e.which < 193) || // ;=,-./` (in order)
                    (e.which > 218 && e.which < 223);   // [\]' (in order)
                var isActionKey = (e.which > 111 && e.which < 124) // f1-f12
                var isComboKey = e.which == 229 //android input placeholder
                var isMod = e.ctrlKey || e.altKey

                if ((isChar || isComboKey) && !isMod) {
                    removeDefault()
                    if (isComboKey && isDef) {
                        isComboStarted = true
                    }
                }
                else if (isDef && !isChar && !isMod && !isActionKey) {
                    e.preventDefault()
                }
            })
            .on('keyup', function (e) {
                var isComboKey = e.which == 229

                if (isComboStarted) {
                    isComboStarted = false
                }
                else if(!isComboKey) {
                    setDefault()
                }
            })
            .on('change', function () {
                setDefault()
            })
            .on('paste', function (e) {
                removeDefault()
            })
            .on('drag', function (e) {
                if (input.hasClass('empty'))
                    e.preventDefault()
            })
    }
    function destroy(input) {
        input.off('focus touchstart keydown keyup change paste drag')
    }

    $.fn.inputHint = function (defaultText, command) {
        var targets = this

        for (var i = 0; i < targets.length; i++) {
            var input = $(targets[i])
            
            if (command === 'destroy')
                destroy(input)
            else
                init(input, defaultText)
        }

        return targets
    }
})(jQuery);

//search
function search(text, categoryID) {  
    text = encodeURIComponent(text)
    var url = string_Format('/search/index?input={0}', text)
    if (categoryID && categoryID !== '000000000000000000000000')
        url += '&categoryid=' + categoryID

    window.location.href = url
}

//model
var CookieModel = function (params) {
    //params: name, type, path, defaultValue
    var self = this;

    function parseCookie(value) {
        if (params.type === "number") {
            var intVal = parseInt(value);
            if ($.isNumeric(intVal)) {
                return intVal;
            }
            return null;
        }
        else if (params.type === "boolean") {
            if (value === null || value === undefined)
                return null
            
            var isTrue = value.toLowerCase() === 'true' || value === '1'
            if (isTrue)
                return true

            var isFalse = value.toLowerCase() === 'false' || value === '0'
            if (isFalse)
                return false           

            return null
        }
        else {
            return value;
        }
    }

    self.restore = function () {
        var value = $.cookie(params.name)
        value = parseCookie(value)

        if ((value === null || value === undefined)
            && params.defaultValue !== undefined) {
            value = params.defaultValue
        }

        return value;
    }
    self.save = function (value) {
        if (!params.path)
            params.path = window.location.pathname;
        $.cookie(params.name, value, { expires: 365, path: params.path });
    }
}
var DropdownVM = function (params) {
    //params: items, selectedItem, selectedIndex, cookie, valueName

    var self = this;
    self.cookie = params.cookie;
    self.items = ko.observableArray(params.items);
    self.renderedItem = null;

    //methods
    var pickSelected = function () {
        if (!params.valueName)
            params.valueName = 'value';

        if (params.cookie) {
            var cookieValue = self.cookie.restore();

            if (cookieValue) {
                for (var i = 0; i < params.items.length; i++) {
                    if (params.items[i][params.valueName] === cookieValue) {
                        self.nextItem = params.items[i];
                        break;
                    }
                }
            }
        }

        if (!self.nextItem && params.selectedItem) {
            self.nextItem = params.selectedItem
        }

        if (!self.nextItem && params.items && params.items.length > 0) {
            if (!params.selectedIndex) {
                params.selectedIndex = 0
            }

            self.nextItem = params.items[params.selectedIndex]
        }

        if (self.nextItem) {
            self.renderedItem = ko.observable(self.nextItem);
        }
    }

    self.change = function (item) {
        self.nextItem = item;
        self.renderedItem(item);
    }

    self.syncRendered = function () {
        self.renderedItem(self.nextItem);

        if (self.cookie) {
            var value = self.nextItem[params.valueName];
            self.cookie.save(value)
        }
    }

    //init
    pickSelected();
}

//common string + time
function string_Format() {
    var s = arguments[0];
    for (var i = 0; i < arguments.length - 1; i++) {
        var reg = new RegExp("\\{" + i + "\\}", "gm");
        s = s.replace(reg, arguments[i + 1]);
    }

    return s;
}
function time_ToLocals(elements, toFormat) {
    if (!toFormat)
        toFormat = momentZoneFormat;

    $.each(elements, function (index, item) {
        item = $(item);

        var text = item.attr("data-time") || item.text()
        var itemFormat = item.attr("data-format") || toFormat
        var utcString = $.trim(text);
        var momentDate = moment.utc(utcString).local();

        if (momentDate.isValid()) {
            var localString = momentDate.format(itemFormat);
            item.text(localString).css({
                "visibility": "inherit",
                "width": "auto",
            })
        }
    });
}
function time_ToLocal(utcString, toFormat) {    
    if (!toFormat)
        toFormat = momentZoneFormat;

    var momentDate = moment.utc(utcString).local();

    return momentDate.isValid()
        ? momentDate.format(toFormat)
        : utcString;
}