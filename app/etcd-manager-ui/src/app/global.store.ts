import { inject, InjectionToken } from '@angular/core';
import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';
import { TreeNode } from 'primeng/api';

interface CurrentUser {
  id: string;
  name: string;
}

interface SelectedEtcdConnection {
  id: number;
  name: string;
}

interface Connections {
  selectedEtcdConnection: SelectedEtcdConnection;
  dataSource: any[];
}

interface DipslaySidebar {
  connectionManager: boolean;
  userManager: boolean;
  etcdUserManager: boolean;
  etcdRoleManager: boolean;
  etcdSnapshotManager: boolean;
}

interface KeyValues {
  dataSource: any[];
  treeDataSource: TreeNode[];
  selectedKey: string;
  defaultNewKey: string | null;
  isNewState: boolean;
  treeLoading: boolean;
  newKeySuccessAt: Date | null;
  renameKeySuccessAt: Date | null;
  deleteSuccessAt: Date | null;
}

interface GlobalStoreState {
  currentUser: CurrentUser;
  connections: Connections;
  readyRenderPage: boolean;
  topMenuActive: boolean;
  dipslaySidebar: DipslaySidebar;
  keyValues: KeyValues;
}

const initialState: GlobalStoreState = {
  currentUser: {
    id: '',
    name: '',
  },
  connections: {
    selectedEtcdConnection: {
      id: -1,
      name: ''
    },
    dataSource: [] as any[],
  },
  readyRenderPage: false,
  topMenuActive: false,
  dipslaySidebar: {
    connectionManager: false,
    userManager: false,
    etcdUserManager: false,
    etcdRoleManager: false,
    etcdSnapshotManager: false
  },
  keyValues: {
    dataSource: [] as any[],
    treeDataSource: [] as TreeNode[],
    selectedKey: '',
    defaultNewKey: null,
    isNewState: false,
    treeLoading: false,
    newKeySuccessAt: null,
    renameKeySuccessAt: null,
    deleteSuccessAt: null,
  }
};

const GLOBAL_STATE = new InjectionToken<GlobalStoreState>('GlobalState', {
  factory: () => initialState,
});

export const globalStore = signalStore(
  { providedIn: 'root' },
  withState(() => inject(GLOBAL_STATE)),
  withMethods((store) => ({
    closeSidebar(): void {
      patchState(store, {
        dipslaySidebar: {
          connectionManager: false,
          userManager: false,
          etcdUserManager: false,
          etcdRoleManager: false,
          etcdSnapshotManager: false
        }
      });
    },
    clickPage(page: string): void {
      console.log('click page', page);
      const sidebar = store.dipslaySidebar();
      switch (page) {
        case 'connectionManager':
          patchState(store, { dipslaySidebar: { ...sidebar, connectionManager: !sidebar.connectionManager } });
          break;
        case 'userManager':
          patchState(store, { dipslaySidebar: { ...sidebar, userManager: !sidebar.userManager } });
          break;
        case 'etcdUserManager':
          patchState(store, { dipslaySidebar: { ...sidebar, etcdUserManager: !sidebar.etcdUserManager } });
          break;
        case 'etcdRoleManager':
          patchState(store, { dipslaySidebar: { ...sidebar, etcdRoleManager: !sidebar.etcdRoleManager } });
          break;
        case 'etcdSnapshotManager':
          patchState(store, { dipslaySidebar: { ...sidebar, etcdSnapshotManager: !sidebar.etcdSnapshotManager } });
          break;
      }
    },
    setCurrentUser(user: CurrentUser): void {
      patchState(store, { currentUser: user });
    },
    setDataSourceConnections(dataSource: any[]): void {
      patchState(store, { connections: { ...store.connections(), dataSource } });
    },
    setDataSourceKeys(dataSource: any[]): void {
      patchState(store, { keyValues: { ...store.keyValues(), dataSource } });
    },
    setReadyRenderPage(ready: boolean): void {
      patchState(store, { readyRenderPage: ready });
    },
    selectedEtcdConnection(selectedEtcdConnection: SelectedEtcdConnection): void {
      patchState(store, { connections: { ...store.connections(), selectedEtcdConnection } });
    },
    setIsNewState(isNewState: boolean): void {
      patchState(store, { keyValues: { ...store.keyValues(), isNewState } });
    },
    setSelectedKey(selectedKey: string): void {
      patchState(store, { keyValues: { ...store.keyValues(), selectedKey } });
    },
    setDefaultNewKey(defaultNewKey: string | null): void {
      patchState(store, { keyValues: { ...store.keyValues(), defaultNewKey } });
    },
    setRenameKeySuccessAt(renameKeySuccessAt: Date | null): void {
      patchState(store, { keyValues: { ...store.keyValues(), renameKeySuccessAt } });
    },
    setDeleteSuccessAt(deleteSuccessAt: Date | null): void {
      patchState(store, { keyValues: { ...store.keyValues(), deleteSuccessAt } });
    },
    setNewKeySuccessAt(newKeySuccessAt: Date | null): void {
      patchState(store, { keyValues: { ...store.keyValues(), newKeySuccessAt } });
    },
    setTreeDataSource(treeDataSource: TreeNode[]): void {
      patchState(store, { keyValues: { ...store.keyValues(), treeDataSource } });
    },
    setTreeLoading(treeLoading: boolean): void {
      patchState(store, { keyValues: { ...store.keyValues(), treeLoading } });
    }
  }))
);
