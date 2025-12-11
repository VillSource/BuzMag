import { loadRemoteModule } from '@angular-architects/native-federation';
import { Routes } from '@angular/router';

export const routes: Routes = [
    {
        path: '',
        loadChildren: () =>
            loadRemoteModule({
                remoteName: 'user', 
                exposedModule: './Component' 
            }).then(m => m.App)
    }
];
