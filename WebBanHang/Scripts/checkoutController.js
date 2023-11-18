





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
    ,
    AddCoupon: (code, ck) => {
        console.log(code,ck);
        $.ajax({
            data: { code, ck, cps: ck.coupons},
            type: "post",
            url: `/Checkout/AddCoupon`,
            datatype: "json",
            cache: false,
            success: function (res) {
                console.log(res)
                
                if (!res) {
                    alert("Mã không áp dụng cho các sản phẩm trong giỏ hàng !");
                    return;
                }
                else {
                    $("#checkout-sum").html(res);
                    console.log(res);
                    //$('#checkout-sum').append(res);
                }
            },
            error: function (err) {
                console.log(err.responseText)
                alert("CO LOI XAY RA");
            }
        });
    },
    RemoveCoupon: (code, ck) => {
        console.log(ck);
        $.ajax({
            data: { code, ck },
            type: "post",
            url: `/Checkout/RemoveCoupon`,
            datatype: "json",
            cache: false,
            success: function (res) {
                console.log(res)
                if (!res) {
                    alert("Mã không áp dụng cho các sản phẩm trong giỏ hàng !");
                    return;
                }
                else {
                    $("#checkout-sum").html(res);
                    console.log(res);
                    //$('#checkout-sum').append(res);
                }
            },
            error: function (err) {
                console.log(err.responseText)
                alert("CO LOI XAY RA");
            }
        });
    }
}