import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { RemotePice } from "./core/remote-pice/remote-pice";

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RemotePice],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('BuzMag');
}
