'use strict';

(function EventListeners() {
    // Select all buttons on the products 
    var plus = document.getElementsByName("pQuantity");
    var minus = document.getElementsByName("mQuantity");
    var basket = document.getElementsByName("addToBasket");

    var plusValue = 0;
    var minusValue = 0;
    var basketValue = 0;
    // add id to all 'plus quantity' buttons
    for (var i = 0; i < plus.length; i++) {
        plus[i].setAttribute("id", "P" + plusValue);
        plusValue++;
    }
    // add id to all 'minus quantity' buttons
    for (var j = 0; j < minus.length; j++) {
        minus[j].setAttribute("id", "M" + minusValue);
        minusValue++;
    }
    // add id to all 'add to basket' buttons
    for (var k = 0; k < minus.length; k++) {
        basket[k].setAttribute("id", "B" + basketValue);
        basketValue++;
    }
})();

function minusQuantity(clicked_id) {
    // select product id and quantity
    var pos = clicked_id.charAt(clicked_id.length - 1);
    var number = parseInt($('span.badge')[pos].innerText);
    // decrease the quantity and show result
    if (number <= 1) {
        number = 1;
    }
    else {
        number -= 1;
    }

    $('span.badge')[pos].innerText = number;
}

function plusQuantity(clicked_id) {
    // select product id and quantity
    var pos = clicked_id.charAt(clicked_id.length - 1);
    var number = parseInt($('span.badge')[pos].innerText);
    // increase the quantity and show result
    if (number >= 10) {
        number = 10;
    }
    else {
        number += 1;
    }

    $('span.badge')[pos].innerText = number;
}

function AddToBasket(clicked_id, productID) {
    // select quantity and product id
    var pos = clicked_id.charAt(clicked_id.length - 1);
    var quantity = $('span.badge')[pos].innerText;
    // place the product and quantity into session storage
    if (quantity != 0 || quantity != null) {
        var basketItem = { 'product': productID, 'quantity': quantity };
        sessionStorage.setItem('basketItem' + productID, JSON.stringify(basketItem));

        toastr["success"]('Added to basket!');
        toastr.options = { "showMethod": "fadeIn", "hideMethod": "fadeOut", "timeOut": "3000", "closeButton": true };
    }
}

function DisplayBasket() {
    // collect all the keys in storage
    var keys = [];
    var allKeys = "";
    

    for (var i = 0; i < sessionStorage.length; i++) {
        keys[i] = sessionStorage.key(i);
    }

    sessionStorage.setItem("allKeys", JSON.stringify(keys));

    window.location.href = "cart.cshtml";
}