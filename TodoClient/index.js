const todoApiUrl = "http://10.42.0.109:5122/api/TodoItems";
const remainingItemsSection = document.querySelector(".todo-items__remaining");
const doneItemsSection = document.querySelector(".todo-items__done");
const addTodoItem = document.querySelector(".todo-item__input-container > button");

const todoItemTemplate = (id, name, isChecked) => `
<section data-id="${id}" class="todo-item">
  <h1 class="todo-item__id">#${id}</h1>
  <p contenteditable="true" class="todo-item__name">${name}</p>
  <div class="todo-item__save-container">
    <div class="todo-item__is-completed">
      <label for="isCompleted${id}">Done: </label>
      <input ${isChecked} type="checkbox" name="${id}">
    </div>
    <input type="button" name="${id}" value="Save" class="todo-item__save button">
  </div>
</section>
`;

async function getTodoItems(url) {
  let response = await fetch(url);

  if (!response.ok) {
    throw new Error(`Cannot retrieve todo items, status: ${response.status}`);
  }
  let data = await response.json();
  return data;
}

async function putItemToServer(event) {
  const saveButton = event.currentTarget;

  const itemPojo = {};
  itemPojo["id"] = saveButton.getAttribute("name");
  const item = document.querySelector(`.todo-item[data-id="${itemPojo["id"]}"]`);
  itemPojo["name"] = item.children[1].innerText;
  itemPojo["isComplete"] = document.querySelector(
    `.todo-item__is-completed input[name='${itemPojo["id"]}']`
  ).checked;

  fetch(todoApiUrl, {
    method: "PUT",
    headers: {"Content-Type": "application/json"},
    body: JSON.stringify(itemPojo),
  })
    .then((response) => {
      if (!response.ok) {
        throw Error(
          "Could not complete PUT request to server, status: ",
          response.status
        );
      }
      console.log(response.status);
      // reload page to refresh UI
      window.location.reload();
    })
    .catch((error) => {
      console.error("Error:", error);
    });
}

async function postItemToServer() {
  const nameInput = document.querySelector(".todo-item__input-container > textarea");

  const itemPojo = {};
  itemPojo["name"] = nameInput.value;
  itemPojo["isCompleted"] = false;

  fetch(todoApiUrl, {
    method: "POST",
    headers: {"Content-Type": "application/json"},
    body: JSON.stringify(itemPojo),
  })
    .then((response) => {
      if (!response.ok) {
        throw Error(
          "Could not complete POST request to server, status: ",
          response.status
        );
      }
      console.log(response.status);
      // reload page to refresh UI
      window.location.reload();
    })
    .catch((error) => {
      console.error("Error:", error);
    });
}

async function loadTodoItems() {
  let items = await getTodoItems(todoApiUrl);

  for (item of items) {
    if (item["isComplete"]) {
      doneItemsSection.insertAdjacentHTML(
        "beforeend",
        todoItemTemplate(item["id"], item["name"], "checked")
      );
    } else {
      remainingItemsSection.insertAdjacentHTML(
        "beforeend",
        todoItemTemplate(item["id"], item["name"], "")
      );
    }
    document
      .querySelector(`.todo-item__save[name='${item["id"]}']`)
      .addEventListener("click", putItemToServer);
  }
}

document.addEventListener("DOMContentLoaded", loadTodoItems);
addTodoItem.addEventListener("click", postItemToServer);
