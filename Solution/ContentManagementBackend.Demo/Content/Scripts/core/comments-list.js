//var CaptchaContainer;
//var CommentCommands;
//var CommandsClass = "com-commands";

function Comments(settings) {
    var OpenedReply = function (itemVM, sender) {
        var self = this
        self.itemVM = itemVM
        self.sender = sender

        var getID = function () {
            return settings.inputAccessor.findReplyInputId(itemVM, sender)
        }

        self.create = function () {
            itemVM.inputVisible(true)
           
            var id = getID()
            var initialText = self.getInitialReply()
            settings.inputAccessor.create(id, true, initialText)
        }

        self.remove = function () {
            itemVM.inputVisible(false)
            itemVM.controls.clear()

            var id = getID()
            settings.inputAccessor.remove(id)
        }

        self.getContent = function(){
            var id = getID()
            return settings.inputAccessor.getContent(id)
        }

        self.clear = function () {
            var id = getID()
            settings.inputAccessor.setContent(id, '')
        }

        self.getInitialReply = function () {
            return '+' + itemVM.comment.authorName + '&nbsp;'
        }

        self.create()
        return self
    }
    var ControlsVM = function () {
        var self = {
            email: ko.observable(),
            name: ko.observable(),            
            message: ko.observable()
        }

        self.clear = function () {
            self.email('')
            self.name('')
            self.message('')
        }

        return self
    }
    var CommentVM = function (comment, branchRoot) {
        return {
            comment: comment,
            branchRoot: branchRoot,
            children: ko.observableArray(),
            inputVisible: ko.observable(false),
            controls: new ControlsVM()
        }
    }
    var StructureMapper = function (rootObservableArray) {
        var self = this
        self.root = rootObservableArray

        self.mapList = function (list) {
            var vmList = []

            for (var i = 0; i < list.length; i++) {
                var rootItem = list[i]
                var rootVM = new CommentVM(rootItem, null)
                vmList.push(rootVM)

                if (rootItem.children) {                   
                    for (var c = 0; c < rootItem.children.length; c++) {
                        var childVM = new CommentVM(rootItem.children[c], rootVM)
                        rootVM.children.push(childVM)
                    }
                }
            }

            self.root(vmList)
        }

        self.add = function (newItem, itemToReply) {
            if (itemToReply) {
                var rootVM = itemToReply.itemVM.branchRoot || itemToReply.itemVM
                var itemVM = new CommentVM(newItem, rootVM)
                rootVM.children.push(itemVM)
            }
            else {
                var itemVM = new CommentVM(newItem, null)
                self.root.unshift(itemVM)
            }
        }

        return self
    }

    var vm = {
        items: ko.observableArray(),
        isInitialLoad: ko.observable(false),
        isLoading: ko.observable(false),
        userData: ko.observable(settings.userData),
        controls: new ControlsVM()
    }
    var openedReply
    var mapper = new StructureMapper(vm.items)


    //action
    vm.actionCancel = function () {
        if (vm.isLoading())
            return;

        vm.controls.clear()
        settings.inputAccessor.setContent(settings.elements.defaultInputID, '')
        document.body.focus()
    }
    vm.actionAdd = function (commentsVM, e) {
        var content = settings.inputAccessor.getContent(settings.elements.defaultInputID);       
        if (!content || vm.isLoading())
            return

        var data = {
            content: $('<div/>').text(content).html()
        }

        if (!vm.userData()) {
            data.email = vm.controls.email()
            data.name = vm.controls.name()
        }

        queryAdd(data)
    }
    vm.actionToggleReplyForm = function (itemVM, e) {
        if (vm.isLoading())
            return;

        var isSameAsOpened = openedReply && openedReply.itemVM === itemVM

        if (openedReply) {
            openedReply.remove()
            openedReply = null
        }

        if (!isSameAsOpened) {
            openedReply = new OpenedReply(itemVM, e.target)
        }

        raiseEvent('onToggleReplyForm', null, {
            isClosing: isSameAsOpened,
            item: itemVM,
            event: e
        })
    }
    vm.actionCancelReply = function (itemVM, e) {
        if (vm.isLoading())
            return

        if (openedReply) {
            openedReply.remove()
            openedReply = null
        }
    }
    vm.actionAddReply = function (itemVM, e) {
        if (vm.isLoading() || !openedReply)
            return

        var content = openedReply.getContent()
        if (!content)
            return;

        var data = {
            content: $('<div/>').text(content).html(),
            parentCommentID: itemVM.comment.commentID,
            branchCommentID: itemVM.comment.branchCommentID || itemVM.comment.commentID
        }

        if (!vm.userData()) {
            data.email = itemVM.controls.email()
            data.name = itemVM.controls.name()
        }

        queryReply(data)
    }
    
    //ajax query
    var queryList = function () {
        vm.isLoading(true);

        ajaxRequest(
            settings.urls.list,
            {
                contentID: settings.contentID,
                categoryID: settings.categoryID
            },
            applyList)
    }
    var queryAdd = function (data) {
        vm.isLoading(true);

        data.contentID = settings.contentID

        ajaxRequest(
            settings.urls.add,
            data,
            applyAdd)
    }
    var queryReply = function (data) {
        vm.isLoading(true);

        data.contentID = settings.contentID

        ajaxRequest(
           settings.urls.add,
           data,
           applyReply)
    }
    var queryDelete = function (comment) {
        vm.isLoading(true);

        ajaxRequest(
            settings.urls.list,
            {
                contentID: settings.contentID,
                comment: comment.commentID                
            },
            applyDelete)
    }
    var queryVote = function (comment, vote) {
        vm.isLoading(true);

        ajaxRequest(
            settings.urls.vote,
            {
                contentID: settings.contentID,
                comment: comment.commentID,
                vote: vote
            },
            applyVote)
    }
    
    //apply response
    var applyAddBase = function (data, hasError) {
        vm.isInitialLoad(false);
        vm.isLoading(false)

        if (data && data.user) {
            vm.userData(data.user)

            vm.controls.clear()
            if (openedReply)
                openedReply.itemVM.controls.clear()

            raiseEvent("onAuth", null, data)
        }

        if (hasError) {
            return false
        }

        if (openedReply) {
            openedReply.itemVM.controls.message(data.message)
        }
        else {
            vm.controls.message(data.message)
        }

        if (data.message) {            
            return false
        }

        return true
    }
    var applyList = function (data, hasError) {
        vm.isInitialLoad(false)
        vm.isLoading(false)
        if (hasError)
            return
       
        var list = raiseEvent("onListResponse", null, data) || data        
        if (list) {
            mapper.mapList(list)
        }
    }
    var applyAdd = function (data, hasError) {
        if (!applyAddBase(data, hasError)) {
            return
        }

        settings.inputAccessor.setContent(settings.elements.defaultInputID, '')

        var item = raiseEvent('onAddResponse', null, data) || data
        if (item) {
            mapper.add(item)        
        }
    }
    var applyReply = function (data, hasError) {
        if (!applyAddBase(data, hasError)) {
            return
        }

        var item = raiseEvent('onAddResponse', null, data) || data
        if (item) {
            mapper.add(item, openedReply)
            
            openedReply.remove()
            openedReply = null
        }
    }
    var applyDelete = function (data, hasError) {
        vm.isLoading(false);

        //msgContainer.html(data.result);
        //SetCommentIsDeleted(msgContainer, true);
        //CheckIfVoteable(msgContainer);
    }
    var applyVote = function (data, hasError) {
        vm.isLoading(false);

        //if (!hasError)
        //    PlaceVoteBar(voteBtn, data.VotesUp, data.VotesDown);
    }
    
    //common
    var raiseEvent = function(name, thisObj) {
        if (settings.events[name]) {
            return settings.events[name].apply(
                    thisObj || vm,
                    Array.prototype.slice.call(arguments, 2)
                );
        }
    }
    var navigateToComment = function () {
        var urlParts = window.location.href.split("#");
        if (urlParts.length > 1) {
            var hash = "#" + urlParts[urlParts.length - 1];
            $(document.body).animate({
                'scrollTop': $(hash).offset().top
            }, 2000);
        }
    }


    //init
    settings.inputAccessor.create(settings.elements.defaultInputID, false, null);

    if (settings.initialRequest) {
        vm.isInitialLoad(true)
        queryList()
    }

    return vm;


    // vote
    function PlaceVoteNumber(numberSpan, votesUp, votesDown) {
        if (!numberSpan.hasClass("com-votes-num"))
            numberSpan = GetCommentContainer(numberSpan).children(".com-author-content").find(".com-votes-num");
        numberSpan.removeClass("color-red color-green color-gray");
        var voteable = CheckIfVoteable(numberSpan);

        if (voteable) {
            if (!$.isNumeric(votesUp))
                votesUp = 0;
            if (!$.isNumeric(votesDown))
                votesDown = 0;
            var totalNumber = votesUp + votesDown;
            var diffNumber = votesUp - votesDown;

            if (diffNumber > 0) {
                numberSpan.addClass("color-green");
                numberSpan.html("+" + diffNumber);
            }
            else if (diffNumber < 0) {
                numberSpan.addClass("color-red");
                var numberStr = (diffNumber + "").replace("-", "–");
                numberSpan.html(numberStr);
            }
            else {
                numberSpan.addClass("color-gray");
                numberSpan.html("0");
            }
            numberSpan.attr("title", "Всего " + totalNumber + ": ↑" + votesUp + " и ↓" + votesDown);

            var textContainer = numberSpan.closest(".com-wrapper").children(".com-text-container");
            textContainer.css({ opacity: "" });
            textContainer.unbind("hover");
            if (diffNumber <= DownVoteToGray) {
                textContainer.css({ opacity: 0.2 });
                textContainer.hover(
                    function () { $(this).fadeTo('slow', 1); },
                    function () { $(this).fadeTo('slow', 0.2); });
            }
        }
    }
    function PlaceVoteBar(likebar, votesUp, votesDown) {
        if (!likebar.hasClass("com-like-graf"))
            likebar = GetCommentContainer(likebar).children(".com-author-content").find(".com-like-graf");
        var voteable = CheckIfVoteable(likebar);

        if (voteable) {
            if (!$.isNumeric(votesUp))
                votesUp = 0;
            if (!$.isNumeric(votesDown))
                votesDown = 0;
            var totalNumber = votesUp + votesDown;
            var diffNumber = votesUp - votesDown;
            var UpPersent = votesUp / totalNumber * 100;
            var DownPersent = 100 - UpPersent;

            if (totalNumber > 0) {
                likebar.show();
                likebar.find(".com-like").css({ width: UpPersent + "%" });
                likebar.find(".com-dislike").css({ width: DownPersent + "%" });
                likebar.find(".com-liketext").find(".comment_like_text").text(votesUp);
                likebar.find(".com-liketext").find(".comment_dislike_text").text(votesDown);

                var textContainer = likebar.closest(".com-wrapper").children(".com-text-container");
                textContainer.css({ opacity: "" });
                textContainer.unbind("hover");
                if (diffNumber <= DownVoteToGray) {
                    textContainer.css({ opacity: 0.4 });
                    textContainer.hover(
                        function () { $(this).fadeTo('slow', 1); },
                        function () { $(this).fadeTo('slow', 0.4); });
                }
                //-moz-opacity: 0.4;filter: alpha(opacity=40);
            }
            else {
                likebar.hide();
            }
        }
    }
    function CheckIfVoteable(likebar) {
        if (!likebar.hasClass("com-like-graf"))
            likebar = GetCommentContainer(likebar).children(".com-author-content").find(".com-like-graf");
        var isDeleted = GetCommentIsDeleted(likebar);
        if (isDeleted == true)
            $(likebar).parent().hide();
        return isDeleted == false;
    }

      
    // capchta
    function CreateCaptchaInForm(commentContainer) {
        var addForm = commentContainer.children(".com-add-form");
        var errorMsg = addForm.find(".color-red");
        var parentId = GetCommentId(commentContainer);
        var captchaId = "comments-captcha" + parentId;
        var captchaContainer = addForm.children("#" + captchaId);
        if (captchaContainer.length == 0)
            $("<div id='" + captchaId + "'></div>").insertAfter(errorMsg);

        CreateCaptcha(captchaId);
    }
    function CreateCaptcha(captchaId) {
        var exist = CaptchaContainer == captchaId;
        if (exist) {
            RefreshCaptcha();
        }
        else {
            if (CaptchaContainer) {
                $("#" + CaptchaContainer).html('');
                CaptchaContainer = null;
            }
            CaptchaContainer = captchaId;
            Recaptcha.create("6LdexuASAAAAAESSowR682yM4IcCbu0begC8rNCr", captchaId, {
                theme: 'red',
                callback: Recaptcha.focus_response_field
            });
        }
    }
    function DeleteCaptcha(captchaId) {
        $("#" + captchaId).html("");
        if (CaptchaContainer == captchaId)
            CaptchaContainer = null;
    }
    function GetCaptchaChallenge(container) {
        return Recaptcha.get_challenge();
        //var src = $(container).find("#recaptcha_image").find("img").attr("src");
        //if (src)
        //    return src.slice(44, src.length);
        //else
        //    return null;
    }
    function GetCaptchaResponse(container) {
        return Recaptcha.get_response();
        //var response = $(container).find("#recaptcha_response_field").val();
        //if (response)
        //    return response;
        //else
        //    return null;
    }
    function SendCaptcha(container) {
        var src = $(container).find("#recaptcha_image").find("img").attr("src");
        src = src.slice(44, src.length);
        var text = $(container).find("#recaptcha_response_field").val();

        $.ajax
        ({
            type: 'POST',
            url: '/Handlers/PostHandler.ashx',
            data: {
                challengeValue: src,
                responseValue: text
            },
            dataType: 'html',
            success: function (msg) {

            }
        });
    }
    function RefreshCaptcha(container) {
        Recaptcha.reload();
        //$(container).find("#recaptcha_reload").click();
    }

    // commands
    function InitCommands() {
        var del = {
            text: "Удалить",
            handler: DeleteComment,
            condition: function (commentContainer) {
                var userName = GetUserName(commentContainer);
                var deleted = GetCommentIsDeleted(commentContainer);

                var msMinute = 60 * 1000;
                var nowTime = new Date().getTime();
                var addTime = new Date(commentContainer.find(".timeago").eq(0).attr("data-title"));
                var minutesDiff = Math.floor((nowTime - addTime) / msMinute);
                var inTime = minutesDiff <= MaxCommentDeleteTime;

                return !deleted && userName == UserName && inTime;
            }
        }
        CommentCommands = new Array();
        CommentCommands.push(del);

        $("html").click(function (e) {
            $("." + CommandsClass).each(function () {
                var hide = false;
                var time = $(this).attr("data-opened");
                if (time) {
                    var diff = $.now() - time;
                    hide = diff > 200;
                }
                else {
                    hide = true;
                }

                if (hide) {
                    $(this).closest(".com-wrapper").not(':hover').find(".com-com-button").hide();
                    DeleteCommands(this);
                }
            });
        });
    }
    function BindCommandShow() {
        $(".com-wrapper").unbind("hover");
        $(".com-com-button").unbind("click");

        $(".com-wrapper").hover(function () {
            var commentContainer = $(this).closest(".com-author-container");
            var commandsNeeded = CheckCommandConditions(commentContainer);
            if (commandsNeeded)
                $(this).find(".com-com-button").show();
        },
        function () {
            var container = $(this).closest(".com-wrapper");
            var commandsCreated = container.find("." + CommandsClass).length > 0;
            if (!commandsCreated)
                $(this).find(".com-com-button").hide();
        });

        $(".com-com-button").click(function () {
            var container = $(this).closest(".com-wrapper");
            CreateCommands(container);
        });
    }
    function CheckCommandConditions(container) {
        var commandsAdded = false;
        for (var i = 0; i < CommentCommands.length; i++) {
            var add = CommentCommands[i].condition(container);
            if (add)
                return true;
        }
        return false;
    }
    function CreateCommands(container) {
        var commandsNeeded = CheckCommandConditions(container);
        if (commandsNeeded && container.find("." + CommandsClass).length == 0) {
            var commandContainer = $(document.createElement('div'));
            commandContainer.addClass(CommandsClass);
            commandContainer.attr("data-opened", $.now());
            container.append(commandContainer);

            var list = $(document.createElement('ul'));
            commandContainer.append(list);

            for (var i = 0; i < CommentCommands.length; i++) {
                var add = CommentCommands[i].condition(container);
                if (add) {
                    var li = $(document.createElement('li'));
                    li.html('<div>' + CommentCommands[i].text + '</div>');
                    li.click(CommentCommands[i].handler);
                    list.append(li);
                    commandsAdded = true;
                }
            }
        }
    }
    function DeleteCommands(commandContainer) {
        $(commandContainer).find("li").unbind("click");
        commandContainer.remove();
    }
}
