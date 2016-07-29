var supportedLangs = ['ru'];
var connectionException = 'There was an error while connecting to the server.';

$(function () {
    toastr.options = {
        'timeOut': 4000,
        'positionClass': 'toast-bottom-right'
    }
});

var PagedListVM = function () {
    var self = this;
    var nextPage = 1;
    self.currentPage = ko.observable(1);
    self.totalPages = ko.observable(0);
    self.pagerVisible = ko.observable(false);
    self.isInitialLoad = ko.observable(true);
    self.isLoading = ko.observable(false);
    self.isFirstPage = ko.observable(false);
    self.isLastPage = ko.observable(false);
    self.items = ko.observableArray();

    //methods
    self.moveToStart = function () {
        if (self.isFirstPage())
            return;

        self.moveToPage(1);
    }

    self.moveToEnd = function () {
        if (self.isLastPage())
            return;

        var total = self.totalPages();
        var current = self.currentPage();
        if (current > total)
            return;

        moveToPage(total);
    }

    self.movePrev = function () {
        if (self.isFirstPage())
            return;

        var page = self.currentPage();
        page--;
        if (page < 1)
            page = 1;

        self.moveToPage(page);
    }

    self.moveNext = function () {
        if (self.isLastPage())
            return;

        var page = self.currentPage();
        page++;

        self.moveToPage(page);
    }

    self.moveToPage = function (page) {
        if (self.isLoading())
            return;

        nextPage = page;
        self.isLoading(true);
        self.queryItems(page, self.receiveItems);
    }
    

    //receive result
    self.receiveItems = function (data, hasError) {
        self.isLoading(false);
        self.isInitialLoad(false);

        if (hasError)
            return;

        self.currentPage(nextPage);
        self.isFirstPage(nextPage === 1);

        self.renderItems(data);

        var isOnlyPage = self.isLastPage() && self.isFirstPage();
        self.pagerVisible(!isOnlyPage);

        if (self.items().length === 0 && self.currentPage() > 1) {
            var totalPages = self.totalPages();
            var moveToPage = totalPages === 0 ? 1 : totalPages;
            self.moveToPage(moveToPage);
        }
    }


    //abstract methods
    self.queryItems = function (page, handler) { }
    self.renderItems = function () { }
}

var DynamicListVM = function () {
    var self = new PagedListVM()
    self.isFirstPage(true)


    //init
    self.loadInit = function (page) {
        if (self.isLoading())
            return

        self.isLoading(true)
        self.queryItems(page, self.receiveItems)
    }

    self.bindScrollItemLoading = function () {
        if ($("body").height() <= $(window).height()) {
            self.moveNext(null, null)
        }

        $(window).scroll(function () {
            if ($(window).scrollTop() == $(document).height() - $(window).height()) {
                self.moveNext(null, null)
            }
        });
    }


    //actions
    self.movePrev = function (page, handler) {
        if (self.isFirstPage() || self.isLoading())
            return

        self.isLoading(true)
        self.queryItems(null, self.receiveItems)
    }

    self.moveNext = function (page, handler) {
        if (self.isLastPage() || self.isLoading())
            return

        self.isLoading(true)
        self.queryItems(null, self.receiveItems)
    }


    //receive result
    self.receiveItems = function (data, hasError) {
        self.isLoading(false)
        self.isInitialLoad(false)

        if (hasError)
            return

        self.renderItems(data)
    }
    
    return self;
}



/*datacontext*/
function currentLang() {
    var curerntLang = window.url('1', window.location.href);
    var validLang = $.inArray(curerntLang, supportedLangs) !== -1;
    return validLang ? curerntLang : null;
}
function isLangSet() {
    var curerntLang = window.url('1', window.location.href);
    var validLang = $.inArray(curerntLang, supportedLangs) !== -1;
    return validLang;
}
var ajaxRequest = function (url, params, handler, type) {
    if (isLangSet())
        url = '/' + currentLang() + url;

    if (!type)
        type = 'POST';

    $.ajax({
        url: url,
        cache: false,
        async: true,
        type: type,
        contentType: 'application/x-www-form-urlencoded; charset=utf-8',
        data: params,
        success: function (data) {
            var hasError = data && data.error ? true : false;

            if (hasError)
                logError(data.error);

            handler(data, hasError);
        }
    })
    .fail(function (xhr, textStatus, err) {
        logError(connectionException);
        handler(null, true);
    });
};

/*toaster*/
function logError(message) {
    toastr.error(message);
}