import { loadRemoteModule } from '@angular-architects/native-federation';
import { Component, Input, ViewChild, ViewContainerRef } from '@angular/core';


@Component({
  selector: 'app-remote-pice',
  imports: [],
  templateUrl: './remote-pice.html',
  styleUrl: './remote-pice.css',
})
export class RemotePice {
  @ViewChild('view', { read: ViewContainerRef, static: true })
  viewContainer!: ViewContainerRef;

  @Input({required:true}) remoteName!: string;
  @Input({required:true}) remoteModule!: string;

  async ngOnInit() {
    try {
      const m = await loadRemoteModule(this.remoteName, this.remoteModule);

      const componentType = m.App;

      this.viewContainer.clear();
      this.viewContainer.createComponent(componentType);
    } catch (error) {
      console.error('Failed to load remote component:', error);
    }
  }
}
