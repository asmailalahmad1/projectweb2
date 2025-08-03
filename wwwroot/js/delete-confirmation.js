// ملف JavaScript لتأكيد الحذف
function confirmDelete(deleteUrl) {
    if (confirm('هل أنت متأكد من أنك تريد حذف هذا العنصر؟ لا يمكن التراجع عن هذا الإجراء.')) {
        // إنشاء نموذج مخفي لإرسال طلب DELETE
        const form = document.createElement('form');
        form.method = 'POST';
        form.action = deleteUrl;
        
        // إضافة رمز مكافحة التزوير
        const token = document.querySelector('input[name="__RequestVerificationToken"]');
        if (token) {
            const hiddenToken = document.createElement('input');
            hiddenToken.type = 'hidden';
            hiddenToken.name = '__RequestVerificationToken';
            hiddenToken.value = token.value;
            form.appendChild(hiddenToken);
        }
        
        // إضافة النموذج إلى الصفحة وإرساله
        document.body.appendChild(form);
        form.submit();
    }
}

// دالة لعرض رسائل التأكيد المخصصة
function showCustomConfirm(message, onConfirm) {
    if (confirm(message)) {
        onConfirm();
    }
}

