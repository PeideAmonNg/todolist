const uri = "api/todo";
let todos = null;
function getCount(data) {
    const el = $("#counter");
    let name = "to-do";
    if (data) {
        if (data > 1) {
            name = "to-dos";
        }
        el.text(data + " " + name);
    } else {
        el.text("No " + name);
    }
}

$(document).ready(function () {
    getData();
});

function getData() {
    $.ajax({
        type: "GET",
        url: uri,
        cache: false,
        success: function (data) {
            const tBody = $("#todos");

            $(tBody).empty();

            getCount(data.length);

            $.each(data, function (key, item) {

                const c = $("<input/>", {
                    type: "checkbox",
                    checked: item.isComplete
                });

                const todoName = $("<span class=todoName></span>").text(item.name);
                const checkbox = $("<span></span>").append(
                   c 
                );

                if (item.isComplete) {
                    todoName.css("text-decoration", "line-through");
                }

                c.change(function () {
                    //alert(this.checked);
                    submitEditedItem(item.id, todoName.text(), this.checked);
                });


                const tr = $("<div class=todoContainer></div>")
                    .append(checkbox)
                    .append(todoName);

                const commands = $(`<div class=commands id=commands-${item.id}></div>`).append(
                    $("<span></span>").append(
                        $(`<i class="fa fa-edit"></i>`).on("click", function () {
                            todoName.css("display", "none");

                            const field = $("<input class=todoNameEdit />", { 
                                type: "text"
                            }).val(todoName.text())



                            const form = $("<form class=editForm></form>").on("submit", function () {
                                //alert(field.val());
                                todoName.text(field.val());
                                submitEditedItem(item.id, field.val(), item.isComplete);
                                return false;
                            });;                        
                            
                            form.append(field);

                            checkbox.after(form);
                            commands.css("display", "none");
                        })
                    )
                )
                    .append(
                        $("<span></span>").append(
                            $('<i class="fa fa-trash-o"></i>').on("click", function () {
                                if (confirm("Delete to-do?")) {
                                    deleteItem(item.id);
                                }
                            })
                        )
                    );


                    tr.append(commands);
                    

                tr.appendTo(tBody);
            });

            todos = data;
        }
    });
}

function addItem() {
    const name = $("#add-name").val()
    if (!name || !name.trim()) {
        return alert("Enter a name for your to-do");
    }

    const item = {
        name: name,
        isComplete: false
    };

    $.ajax({
        type: "POST",
        accepts: "application/json",
        url: uri,
        contentType: "application/json",
        data: JSON.stringify(item),
        error: function (jqXHR, textStatus, errorThrown) {
            alert("Something went wrong!");
        },
        success: function (result) {
            getData();
            $("#add-name").val("");
        }
    });
}

function deleteItem(id) {
    $.ajax({
        url: uri + "/" + id,
        type: "DELETE",
        success: function (result) {
            getData();
        }
    });
}

function editItem(id) {
    $.each(todos, function (key, item) {
        if (item.id === id) {
            $("#edit-name").val(item.name);
            $("#edit-id").val(item.id);
            $("#edit-isComplete")[0].checked = item.isComplete;
        }
    });
    $("#spoiler").css({ display: "block" });
}

function submitEditedItem(id, name, isComplete) {
    const item = {
        name,
        isComplete,
        id
    };

    $.ajax({
        url: uri + "/" + id,
        type: "PUT",
        accepts: "application/json",
        contentType: "application/json",
        data: JSON.stringify(item),
        success: function (result) {
            getData();
        }
    });
    
    return false;
}

$(".my-form").on("submit", function () {
    const item = {
        name: $("#edit-name").val(),
        isComplete: $("#edit-isComplete").is(":checked"),
        id: $("#edit-id").val()
    };

    $.ajax({
        url: uri + "/" + $("#edit-id").val(),
        type: "PUT",
        accepts: "application/json",
        contentType: "application/json",
        data: JSON.stringify(item),
        success: function (result) {
            getData();
        }
    });

    closeInput();
    return false;
});

function closeInput() {
    $("#spoiler").css({ display: "none" });
}