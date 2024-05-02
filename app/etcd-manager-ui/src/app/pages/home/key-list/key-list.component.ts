import { Component, OnInit, ViewChild, effect, inject, signal } from '@angular/core';
import { MenuItem, Message, MessageService, PrimeIcons, TreeNode } from 'primeng/api';
import { ContextMenu, ContextMenuModule } from 'primeng/contextmenu';
import { DialogModule } from 'primeng/dialog';
import { FileUpload, FileUploadModule } from 'primeng/fileupload';
import { ListboxModule } from 'primeng/listbox';
import { ToolbarModule } from 'primeng/toolbar';
import { Tree, TreeModule } from 'primeng/tree';
import { BaseComponent } from '../../../base.component';
import { commonLayoutImport } from '../../../layout/common-layout-import';
import { KeyValueService } from '../../service/key-value.service';
import { patchState } from '@ngrx/signals';

@Component({
  selector: 'app-key-list',
  templateUrl: './key-list.component.html',
  styles: [`
  .key-list-item {
      width: 100%;

      i {
          margin-right: 10px;
      }
  }

  .button-header {
      &:hover {
          cursor: pointer;
      }

    margin-right:20px;
  }

  `],
  standalone: true,
  imports: [...commonLayoutImport, ContextMenuModule, ToolbarModule, ListboxModule, TreeModule,
    DialogModule, FileUploadModule,
  ]
})
export class KeyListComponent extends BaseComponent implements OnInit {

  loaded = signal(false);
  viewMode: 'tree' | 'list' = 'tree';
  parentKeyOnNew = '';
  showNewKeyForm = false;
  showImportNodes = false;
  selectedKey?: string;
  contextMenuSelectedKey?: string;
  treeIsExpandAll = false;
  treeSelectedItem: any = {};
  currentSelectRow: any;
  contextMenuModel: MenuItem[] = this.getContextMenu();
  msgs: Message[] = [];
  @ViewChild('mainTree') mainTree!: Tree;
  @ViewChild('fileControl') fileControl!: FileUpload;

  private _messageService = inject(MessageService);
  private _keyValueService = inject(KeyValueService);

  constructor() {
    super();
    effect(() => {
      const selectedConnection = this.globalStore.connections.selectedEtcdConnection();
      if (selectedConnection && selectedConnection.id > 0) {
        this.bindData(selectedConnection.id);
      }
    });
  }

  ngOnInit() {
  }

  getContextMenu() {
    const menu = [];

    if (this.viewMode == 'tree') {
      menu.push({
        label: 'Create child node',
        icon: 'pi pi-plus',
        command: this.createChildNode.bind(this)
      });
    }

    menu.push(...[
      {
        label: 'Delete',
        icon: 'pi pi-trash',
        command: this.menuDelete.bind(this)
      },]);

    if (this.currentSelectRow) {
      menu.push(
        {
          label: 'Export current node',
          icon: 'pi pi-download',
          command: this.exportCurrentNode.bind(this)
        },
      );

    }

    if (this.viewMode == 'tree') {
      menu.push({
        label: 'Export node and all childs',
        icon: 'pi pi-download',
        command: this.exportAllNodeAndChilds.bind(this)
      });
    }

    return menu;
  }

  menuDelete() {
    // if in tree view mode, enable delete recursive option
    // this._keyValueService.onDelete(this.contextMenuSelectedKey, this.viewMode == 'tree').then(rs => {
    //   if (rs) {
    //     this.refreshList();
    //   }
    // });
  }

  exportAllNodeAndChilds() {
    // this._keyValueService.getByKeyPrefix(this.contextMenuSelectedKey).then(rs => {
    //   if (rs.success) {
    //     this._exportService.exportNodes(rs.data);
    //   } else {
    //     this._messageService.add({ severity: 'error', summary: 'Error', detail: rs.message });
    //   }
    // });
  }

  exportCurrentNode() {
    // this._keyValueService.getByKey(this.currentSelectRow.key).then(rs => {
    //   if (rs.success) {
    //     this._exportService.exportJsonNode(rs.data);
    //   } else {
    //     this._messageService.add({ severity: 'error', summary: 'Error', detail: rs.message });
    //   }
    // }).catch(err => {
    //   this._messageService.add({ severity: 'error', summary: 'Error', detail: err.message });
    // });
  }

  createChildNode() {
    if (this.currentSelectRow) {
      this.parentKeyOnNew = this.currentSelectRow.key + '/';
    } else if (this.viewMode == 'tree' && this.treeSelectedItem) {
      this.parentKeyOnNew = this.treeSelectedItem.data + '/';
    }

    this.showNewKeyForm = true;
  }

  menuRename() {
    console.log('rename');
  }

  async bindData(selectedConnectionId: number) {
    try {
      const ds = await this._keyValueService.getAllKeys(selectedConnectionId);
      console.log('all keys', ds);
      const dataSource = ds.map((x: any) => {
        return { key: x };
      });

      patchState(this.globalStore, { keyValues: { ...this.globalStore.keyValues(), dataSource } });

      if (this.viewMode == 'tree') {
        this.bindDataSourceTree(dataSource);
      }
      this.loaded.update(() => true);
    } catch (err: any) {
      console.error(err);
      this._messageService.add({ severity: 'error', summary: 'Error', detail: err.error.error });
      this.msgs.push({ severity: 'error', summary: 'Error', detail: err.error.error });
    }
  }

  onChangeSelectedKey(evt: any) {
    // this.rootCtx.dispatchEvent('changeSelectedKey', evt.value);
  }

  refreshList() {
    const id = this.globalStore.connections.selectedEtcdConnection.id();
    if (id > 0) {
      this.bindData(this.globalStore.connections.selectedEtcdConnection.id());
      this._messageService.add({ severity: 'success', summary: 'Success', detail: 'Refresh success' });
    }
  }

  switchViewMode() {
    if (this.viewMode == 'list') {
      // list
      this.bindDataSourceTree(this.globalStore.keyValues.dataSource());
      this.viewMode = 'tree';
    } else {
      // tree
      this.viewMode = 'list';
    }
    console.log('switch view mode');
  }

  bindDataSourceTree(dataSource: any[]) {
    const paths = dataSource.map(x => x.key);
    const root: TreeNode<string> = {};
    for (let path of paths) {
      let pathParts = path.split('/');
      let currentNode = root;
      for (let part of pathParts) {
        if (part !== '') {
          if (!currentNode.children) {
            currentNode.children = [];
          }
          let childNode = currentNode.children.find(node => node.label === part);
          if (!childNode) {
            childNode = { label: part, data: pathParts.slice(0, pathParts.indexOf(part) + 1).join('/'), expanded: true, expandedIcon: PrimeIcons.FOLDER_OPEN, icon: PrimeIcons.FOLDER } as TreeNode;
            currentNode.children.push(childNode);
          }
          currentNode = childNode;
        }
      }
    }
    const treeDataSource = root.children || [];
    patchState(this.globalStore, { keyValues: { ...this.globalStore.keyValues(), treeDataSource } });
  }

  nomarlizePathCombine(path: string) {
    if (path.startsWith('//')) {
      path = path.substring(1);
    }
    return path;
  }

  export() {
    // this._keyValueService.getAll().then(rs => {
    //   if (rs.success) {
    //     this._exportService.exportNodes(rs.data);
    //   } else {
    //     this._messageService.add({ severity: 'error', summary: 'Error', detail: rs.message });
    //   }
    // });
  }

  import() {
    if (this.fileControl.basicFileInput) {
      this.fileControl.basicFileInput.nativeElement.click();
    }
  }

  newKey() {
    console.log('newkey');
    this.showNewKeyForm = true;
  }

  onSaveNewKey(evt: any) {
    // this.bindData(this.rootCtx.data.connection);
    this.showNewKeyForm = false;
  }

  showContextMenuViewModeList(menu: ContextMenu, event: MouseEvent, item: any) {
    menu.hide();
    event.preventDefault();
    event.stopPropagation();
    setTimeout(() => {
      console.log('context menu', menu, event, item);
      this.currentSelectRow = item;
      this.contextMenuSelectedKey = item.key;
      menu.toggle(event);
    }, 1);
  }

  contextMenuViewModeTreeSelect(evt: any) {
    this.currentSelectRow = this.globalStore.keyValues.dataSource().find(x => x.key == evt.node.data);
    this.contextMenuSelectedKey = evt.node.data;
    this.contextMenuModel = this.getContextMenu();
  }

  preventMouseDown(event: any) {
    event.preventDefault();
    event.stopPropagation();
  }

  onNodeSelect(evt: any) {
    this.selectedKey = this.treeSelectedItem.data;
  }

  toggleExpandTree(isExpand: boolean) {
    this.treeIsExpandAll = isExpand;
    this.globalStore.keyValues.treeDataSource().forEach(node => {
      this.expandRecursive(node, isExpand);
    });
  }

  handleSelectFile(evt: any) {
    // this._exportService.readDataFromFile(evt.currentFiles[0]).then(rs => {
    //   if (rs) {
    //     console.log('file content', rs);
    //   }
    // });
  }

  importNodes() {
    this.showImportNodes = true;
  }

  private expandRecursive(node: TreeNode, isExpand: boolean) {
    node.expanded = isExpand;
    if (node.children) {
      node.children.forEach(childNode => {
        this.expandRecursive(childNode, isExpand);
      });
    }
  }
}
