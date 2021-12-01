const handleAjaxResult = async (result, modal, table, form) => {
    if (table) {
        table.ajax.reload()
    }
    if (result.success) {
        toastr.success(result.message)
        if (modal) {
            modal.modal('toggle')
        }
        if (form) {
            $(form)[0].reset();
        }
    } else {
        toastr.error(result.message)
    }
}

const swalDeleteWarningMessage = (msg, successMsg, callback) => {
    swal({
        title: 'Are you sure?',
        text: msg,
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!',
        showLoaderOnConfirm: true,
        preConfirm: async (value) => {
            return await callback(value)
        },
        allowOutsideClick: () => !swal.isLoading()
    }).then((result) => {
        if (result.dismiss)
            swal("Cancel", result.message, 'error')
        else
            swal(successMsg, result.message, 'success')
    }).catch((error) => {
        swal("Error", error, 'error')
    })
}