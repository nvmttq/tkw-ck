





var Checkout = {
            
    LoadMethodPayment: (e) => {
        var optionSelected = $("#drTypePayment option:selected").val();
       
        if (optionSelected.toString() === '2') {
            $("#load_form_payment").show();
        } else {
            $("#load_form_payment").hide();
        }
    },
    SetOrder: (fullname, phone, address, city, note) => {
        $.ajax({
            data: { fullname, phone, address, city, note},
            type: "get",
            url: `/Order/PaymentWithPaypal`,
            datatype: "json",
            success: function (res) {
                console.log(res)

            },
            error: function (err) {
                console.log(err.responseText);
                alert("CO LOI XAY RA");
            }
        });
    }
}