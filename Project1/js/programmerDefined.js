'use strict';

(function ImmediateJob() {
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


    document.getElementById('updateBasket').addEventListener("click", UpdateCart, false);
    document.getElementById('checkoutBtn').addEventListener("click", CheckoutPage, false);
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

function redirector() {
    window.location.href = "cart.cshtml";
}

function AddToBasket(clicked_id, productID, productPrice, productName, Image, Description) {
    // select quantity and product id
    var pos = clicked_id.charAt(clicked_id.length - 1);
    var quantity = $('span.badge')[pos].innerText;
    // place the product and quantity into session storage
    if (quantity != null) {

        var basketItem = [productID, quantity, productName, productPrice, Image, Description];
        var prod = new Product(basketItem);

        //var basketItem = { 'ProductID': productID, 'Quantity': quantity };
        sessionStorage.setItem('basketItem' + productID, JSON.stringify(prod));

        toastr.options = { "showMethod": "fadeIn", "hideMethod": "fadeOut", "timeOut": "3000", "closeButton": true };
        toastr["success"]('Added to basket!');
    }
}

function DisplayBasket() {
    // collect all the keys in storage
    GetKeys();
    // Change page to basket page
    redirector();
}

function GetKeys() { // Gets all keys in session storage and stores them in local storage
    var keys = [];
    localStorage.removeItem("allKeys");

    for (var i = 0; i < sessionStorage.length; i++) {
        keys[i] = sessionStorage.key(i);
    }
    localStorage.setItem("allKeys", keys);
}

function CartDisplay() {
    var totalPrice = 0;

    var collectiveProducts = localStorage.getItem("allKeys");
    //alert(collectiveProducts);
    var keys = collectiveProducts.split(',');

    var realKeys = [];
    var realCounter = 0;

    for (var a = 0; a < keys.length; a++) {
        if (keys[a].startsWith('basketItem')) {
            realKeys[realCounter] = keys[a];
            realCounter++;
        }
    }

    //alert(realKeys.length);

    var counter = 1;
    // Display each product in the basket here
    for (var i = 0; i < realKeys.length; i++) {
        var prod = sessionStorage.getItem(realKeys[i]);
        prod = JSON.parse(prod);
        $("div.panel-body").append('<div class="row"><div class="col-xs-2"><img class="img-responsive" src="http://placehold.it/100x70"></div><div class="col-xs-4"><h4 class="product-name"><strong>' + prod.Name + '</strong></h4><h4><small>' + prod.Description + '</small></h4></div><div class="col-xs-6"><div class="col-xs-6 text-right"><h6><strong><div class="pricedProds">' + prod.Price + '</div><span class="text-muted"> x</span></strong></h6></div><div class="col-xs-4"><input type="text" name="basketProds" class="form-control input-sm" value="' + prod.Quantity + '"></div><div class="col-xs-2"><button type="button" class="btn btn-link btn-xs" onclick="RemoveItem(' + prod.ID + ')"><span class="glyphicon glyphicon-trash"> </span></button></div></div></div>');
        var price = prod.Price * prod.Quantity;
        totalPrice += price;
        // Increment counter for each productID in the Basket
        counter++;
    }

    document.getElementById("myPrice").innerHTML = totalPrice;
}

function UpdateCart() {
    var quantities = [];
    var price = 0;

    for (var i = 0; i < document.getElementsByName("basketProds").length; i++) {
        quantities[i] = document.getElementsByName("basketProds")[i].value;
    }

    for (var j = 0; j < $(".pricedProds").length; j++) {
        price += (quantities[j] * parseFloat($(".pricedProds")[j].innerHTML).toFixed(2));
    }

    document.getElementById("myPrice").innerHTML = price;
}

function Product(temp) { // Model for each product in the basket
    this.ID = temp[0];
    this.Quantity = temp[1];
    this.Name = temp[2];
    this.Price = temp[3];
    this.Image = temp[4];
    this.Description = temp[5];
}

function RemoveItem(productID) { // Removes an item from the basket
    sessionStorage.removeItem("basketItem" + productID);
    GetKeys();
    location.reload();
}

function PriceGetter() {
    var price = document.getElementById("myPrice").innerHTML;
    sessionStorage.setItem("price", JSON.stringify(price));
}

function CheckoutPage() { // Sends to the checkout page
    PriceGetter();

    window.location.href = "checkout.cshtml";
}

function ConfirmationPage() {
    ShippingDetails();

    window.location.href = 'ConfirmationPage.cshtml';
}

function OrderDetails(temp) { // Model for each product in the basket
    this.Name = temp[0];
    this.Email = temp[1];
    this.Address1 = temp[2];
    this.Address2 = temp[3]
    this.postCode = temp[4];

}

function ShippingDetails() {
    var name = $('#name').val();
    var email = $('#email').val();
    var address1 = $('#Address1').val();
    var postCode = $('#PostCode').val();

    var details;

    if ($('#Address2').val() != null) {
        var address2 = $('#Address2').val();
        details = [name, email, address1, address2, postCode];
    }
    else {

        details = [name, email, address1, " ", postCode];
    }

    var deets = new OrderDetails(details);

    sessionStorage.setItem('details', JSON.stringify(deets));
}

function ShowDetails() {
    // Get shipping details of customer from sesiion storage  
    var details = JSON.parse(sessionStorage.getItem('details'));
    // Get all the keys that are in session storage 
    var collectiveProducts = localStorage.getItem("allKeys");
    //alert(collectiveProducts);
    var keys = collectiveProducts.split(',');
    // Array for basket items only
    var realKeys = [];
    var realCounter = 0;
    // Loop for separating the products from the rest of the items in session storage
    for (var a = 0; a < keys.length; a++) {
        if (keys[a].startsWith('basketItem')) {
            realKeys[realCounter] = keys[a];
            realCounter++;
        }
    }

    for (var i = 0; i < realKeys.length; i++) {
        var prod = sessionStorage.getItem(realKeys[i]);
        prod = JSON.parse(prod);
        $('#OrderDetails').append('<div>' + prod.Image + '</div><div><strong>' + prod.Name + '</strong></div> <div>Price: &euro;' + prod.Price + '</div>')
    }

    var postcd = details.postCode.toUpperCase();

    $('#shippingAddress').after('<div><strong>' + details.Name + '</strong></div> <div><strong>' + details.Address1 + '</strong></div> <div><strong>' + details.Address2 + '</strong></div> <div><strong>Sligo</strong></div> <div><strong>' + postcd + '</strong></div> <div>Ireland</div>');
}

function BookAppointment(clicked_Name, e) {
    var CategoryName = clicked_Name;

    sessionStorage.setItem('categoryName', JSON.stringify(CategoryName));

    window.location.href = "pay.cshtml";
}

function GetTime(clicked_id) {
    var timeSlot = document.getElementById(clicked_id).value;
    $('#time').val(timeSlot);
}

function ClearTime() {
    $('#time').val("");
}

function GetCategory() {
    var category = sessionStorage.getItem("categoryName").toString();

    $('div h3').append('<p>' + category + '</p>');
}

function DisplayEditBooking(clicked_id, id) {
    var timeSlot = document.getElementById(clicked_id).value;
    $('#datepicker').val(timeSlot);
    $('#bookingId').val(id);
}

function Payment() {
    // Create a Stripe client.
    var stripe = Stripe('pk_test_6pRNASCoBOKtIshFeQd4XMUh');

    // Create an instance of Elements.
    var elements = stripe.elements();

    // Custom styling can be passed to options when creating an Element.
    // (Note that this demo uses a wider set of styles than the guide below.)
    var style = {
        base: {
            color: '#32325d',
            lineHeight: '18px',
            fontFamily: '"Helvetica Neue", Helvetica, sans-serif',
            fontSmoothing: 'antialiased',
            fontSize: '16px',
            '::placeholder': {
                color: '#aab7c4'
            }
        },
        invalid: {
            color: '#fa755a',
            iconColor: '#fa755a'
        }
    };

    // Create an instance of the card Element.
    var card = elements.create('card', { style: style });

    // Add an instance of the card Element into the `card-element` <div>.
    card.mount('#card-element');

    // Handle real-time validation errors from the card Element.
    card.addEventListener('change', function (event) {
        var displayError = document.getElementById('card-errors');
        if (event.error) {
            displayError.textContent = event.error.message;
        } else {
            displayError.textContent = '';
        }
    });

    // Handle form submission.
    var form = document.getElementById('payment-form');
    form.addEventListener('submit', function (event) {
        event.preventDefault();

        stripe.createToken(card).then(function (result) {
            if (result.error) {
                // Inform the user if there was an error.
                var errorElement = document.getElementById('card-errors');
                errorElement.textContent = result.error.message;
            } else {
                // Send the token to your server.
                stripeTokenHandler(result.token);
            }
        });
    });
}