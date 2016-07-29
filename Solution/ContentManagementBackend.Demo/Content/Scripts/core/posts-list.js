$(function () {
    //model
    var PostListVM = function (settings) {
        var self = new DynamicListVM()
        var lastID = settings.lastID
        self.isLastPage(settings.isLastPage)
        self.isInitialLoad(false)

        //methods
        self.queryItems = function (page, handler) {           
            ajaxRequest(
                '/Posts/AjaxList',
                {
                    'lastID': lastID,
                    'categoryIDs': settings.categoryIDs
                },
                handler)
        }

        self.renderItems = function (data) {
            self.isLastPage(!data.isContinued)
            lastID = data.pickID

            if (!data.posts)
                return

            for (var i = 0; i < data.posts.length; i++) {
                var art = data.posts[i]
                art.publishTimeUtc = time_ToLocal(art.publishTimeUtc, momentDateFormat)

                self.items.push(art)
            }
        }
        
        //init
        return self
    }
    
    //init
    $('.al-pagination').hide()
    $('.al-ajaxlist').show()
    
    ko.applyBindings({
        posts: new PostListVM({
            lastID: basePickID,
            isLastPage: isLastPage,
            categoryIDs: categoryIDs
        })
    });
});
