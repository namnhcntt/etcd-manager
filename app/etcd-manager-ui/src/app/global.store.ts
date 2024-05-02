import { signalStore, withState } from '@ngrx/signals';

export const globalStore = signalStore(
  { providedIn: 'root' },
  withState({
    currentUser: {
      id: '',
      name: '',
    },
    selectedEtcdConnectionId: '',
    readyRenderPage: false
  })
);
