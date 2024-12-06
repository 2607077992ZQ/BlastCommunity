// const url = 'http://192.168.2.2/api';
const url = 'http://192.168.3.1:5000/api';
// const url = 'http://127.0.0.1:5000/api';

var addModal = null;
function Load(){
    if(localStorage.length === 0){
        document.getElementById('IsLogin').remove();
    }else{
        document.getElementById('IsNotLogin').remove();
        document.getElementById('navbarDropdown').innerHTML = localStorage.getItem('name')
        document.getElementById('MyaddItem').onclick = () => {
            addModal = new bootstrap.Modal(document.getElementById('addItemModal'))
            addModal.show();
        }
    }
}

function toastalert(err){
    const now = new Date();
    const year = now.getFullYear();
    const month = String(now.getMonth() + 1).padStart(2, '0');
    const day = String(now.getDate()).padStart(2, '0');
    const hours = String(now.getHours()).padStart(2, '0');
    const minutes = String(now.getMinutes()).padStart(2, '0');
    const seconds = String(now.getSeconds()).padStart(2, '0');
    document.getElementById('alertTitle').innerHTML = `${year}/${month}/${day} ${hours}:${minutes}:${seconds}`
    document.getElementById('alertValue').innerHTML = err

    const toastLiveExample = document.getElementById('liveToast')
    const toast = new bootstrap.Toast(toastLiveExample)
    toast.show()
}

//Get请求
async function GetRequest(address){
    try{
        const response = await fetch(address);
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        const jsonlist = await response.json();
        return jsonlist;
    }
    catch(error) {
        throw '访问未达预期';
    }
}

///post
async function PostRequest(address, data) {
    try{
        const response = await fetch(address,{
            method: 'POST',
            headers:{
                'Content-Type': 'application/json'
            },
            body: data === undefined ? null : JSON.stringify(data)
        });
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        const jsonlist = await response.json();
        return jsonlist;
    }
    catch(error) {
        throw '访问未达预期';
    }
}

//生成项目组按钮
function GroupBtn(jsonlist){
    // console.log(jsonlist); // 处理数据
    if(Array.isArray(jsonlist)){
        var btnList = document.getElementById('ItemsGroup')
        var menu = document.getElementById('itemsmenu')
        menu.innerHTML = ''

        jsonlist.forEach(item => {
            var btn = document.createElement('button');
            btn.className = 'btn btn-primary'
            btn.type= 'button'
            btn.textContent = item.ItemName
            
            btn.setAttribute('data-bs-toggle','modal')
            btn.setAttribute('data-bs-target','#staticBackdrop')

            btn.onclick = () => ShowItems(item.Id, btn)
            btnList.append(btn);

            var li_link = document.createElement('li')
            var a_link = document.createElement('a')
            
            a_link.className = 'dropdown-item'
            a_link.href = '#' + item.Id
            a_link.innerHTML = item.ItemName
            a_link.onclick = () => {
                document.getElementById('selectGroup').value = item.ItemName
            }
            li_link.append(a_link)
            menu.append(li_link)

        });
    }
}

//取组
function showGroupList(){
    GetRequest(url + '/Items/GetGroupItems')
        .then(json => {
            GroupBtn(json)
        })
        .catch(error => {
            // console.error('Fetch error:', error);
            toastalert(error);
        })
}

//找组
function findGroupList(search){
    GetRequest(url + '/Items/GetGroupItemsFind?find=' + search)
        .then(json => {
            GroupBtn(json)
        })
        .catch(error => {
            toastalert(error);
        })
}

//查找组事件
function searchgroup(event){
    if(event.key === 'Enter' || event.keyCode === 13){
        var search = document.getElementById('searchgroup').value
        document.getElementById('ItemsGroup').innerHTML = ''

        if(search.trim() != '' ){
            findGroupList(search.trim())
        }else{
            showGroupList()
        }
    }
}

//生成所有项目按钮
function ItemsBtn(jsonList){
    var modaldiv = document.getElementById('Itemsvalues')
    modaldiv.innerHTML = '';

    if(Array.isArray(jsonList)){

        var accordion = document.createElement('div');
        accordion.className = 'accordion'

        for (let i = 0; i < jsonList.length; i++) {
            var group = jsonList[i];

            var accordionName = 'collapse' + i

            var accordionItem = document.createElement('div');
            accordionItem.className = 'accordion-item'

            var accordionHead = document.createElement('h2');
            accordionHead.className = 'accordion-header'

            var accordionBtn = document.createElement('button');
            accordionBtn.className = 'accordion-button'
            accordionBtn.type = 'button'
            accordionBtn.textContent = group[0].sort
            accordionBtn.setAttribute('data-bs-toggle','collapse')
            accordionBtn.setAttribute('data-bs-target','#panelsStayOpen-' + accordionName)
            accordionBtn.setAttribute('aria-expanded','true')
            accordionBtn.setAttribute('aria-controls','panelsStayOpen-' + accordionName)

            var accordionOpen = document.createElement('div')
            accordionOpen.id = 'panelsStayOpen-' + accordionName
            accordionOpen.className = 'accordion-collapse collapse show'
            accordionOpen.setAttribute('aria-labelledby','panelsStayOpen-' + accordionName)

            var accordionbody = document.createElement('div')
            accordionbody.className = 'accordion-body'
            
            var btnGroup = document.createElement('div')
            btnGroup.className = 'd-grid gap-2'

            group.forEach(items => {
                // console.log(items)

                var itemBtn = document.createElement('button');
                itemBtn.className = 'btn btn-primary'
                itemBtn.type = 'button'
                itemBtn.textContent = items.ItemName

                itemBtn.setAttribute('data-bs-toggle','offcanvas');
                itemBtn.setAttribute('data-bs-target','#offcanvasExample');
                itemBtn.setAttribute('aria-controls','offcanvasExample');
                itemBtn.onclick = () => {
                    // console.log(items)
                    document.getElementById('offcanvasExampleLabel').innerHTML = items.ItemName
                    document.getElementById('offcanvas-date').innerHTML = items.createTime
                    document.getElementById('offcanvas-value').innerHTML = items.Item_Remarks
                }

                btnGroup.append(itemBtn)
            })

            accordionbody.append(btnGroup)
            accordionOpen.append(accordionbody)

            accordionHead.append(accordionBtn)
            accordionItem.append(accordionHead)

            accordion.append(accordionItem)
            accordion.append(accordionOpen)
        }
        modaldiv.append(accordion)
    }
}

//进组项目事件
function ShowItems(id, btn){
    document.getElementById('searchgroup').value = ''
    document.getElementById('Itemsvalues').innerHTML = '';
    document.getElementById('modaltitle').innerHTML = btn.innerHTML
    document.getElementById('modalLoding').style.visibility = 'visible';
    document.getElementById('searchitem').setAttribute('name', id);

    GetRequest(url + '/Items/GetItemsList?id=' + id)
        .then(json => {
            // console.log(json)
            ItemsBtn(json)
            document.getElementById('modalLoding').style.visibility = 'hidden';

        })
        .catch(error => {
            toastalert(error);
        })

}

//找项目
function searchItems(event){
    if(event.key === 'Enter' || event.keyCode === 13){
        document.getElementById('Itemsvalues').innerHTML = '';
        document.getElementById('modalLoding').style.visibility = 'visible';
        var search = document.getElementById('searchitem')
        var id = search.name

        if(search.value.trim() != ''){
            GetRequest(url + '/Items/GetItemsListFind?id=' + id + '&find=' + search.value.trim())
                .then(json => {
                    ItemsBtn(json)
                    document.getElementById('modalLoding').style.visibility = 'hidden';
                })
                .catch(error => {
                    toastalert(error);
                })
        }else{
            GetRequest(url + '/Items/GetItemsList?id=' + id)
                .then(json => {
                    // console.log(json)
                    ItemsBtn(json)
                    document.getElementById('modalLoding').style.visibility = 'hidden';

                })
                .catch(error => {
                    toastalert(error);
                })
        }
        

    }

    
}

//验证用户名
function VerifyName(name){
    return PostRequest(url + '/User/Verification?userName=' + name, undefined)
            .then(verify => {
                return verify === true
            })
            .catch(err => {
                throw '用户名验证失败'
            })
}

///添加项目
function AddItemValue(){
    var groupName = document.getElementById('selectGroup')
    var itemname = document.getElementById('addItemName')
    var Remarks = document.getElementById('addItemRe')
    
    if(groupName.value !== '' && itemname.value !== ''){
        var data = {
            UserKey: localStorage.getItem('userKey'),
            GroupName: groupName.value,
            Item: itemname.value,
            Remarks: Remarks.value
        }
        PostRequest(url + '/Items/Contribution', data)
            .then(json => {
                if(json.Code === 200){
                    addModal.hide();

                    groupName.value = ''
                    itemname.value = ''
                    Remarks.value = ''

                    document.getElementById('ItemsGroup').innerHTML = ''
                    showGroupList()
                    toastalert('已保存')
                }else{
                    toastalert(json.Error)
                }
            })
            .catch(err => {
                toastalert(err)
            })

    }else{
        toastalert('组或项目不能留空 操作无效')
    }
    
}