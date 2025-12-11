import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RemotePice } from './remote-pice';

describe('RemotePice', () => {
  let component: RemotePice;
  let fixture: ComponentFixture<RemotePice>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RemotePice]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RemotePice);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
