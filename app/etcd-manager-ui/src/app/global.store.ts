import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';
import { TreeNode } from 'primeng/api';

export const globalStore = signalStore(
  { providedIn: 'root' },
  withState({
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
      selectedKey: ''
    }
  }),
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
    }
  }))
);
