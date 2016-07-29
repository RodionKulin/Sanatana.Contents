$(function () {
    $("#search-trigger").on("click", function (e) {
        var text = $("#search-input").val()
        var categoryID = $("#search-categories option:selected").val();
        
        search(text, categoryID)

        e.preventDefault()
    })    
})