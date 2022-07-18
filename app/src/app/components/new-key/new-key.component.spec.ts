/* tslint:disable:no-unused-variable */
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { DebugElement } from '@angular/core';

import NewKeyComponent from './new-key.component';

describe('NewKeyComponent', () => {
  let component: NewKeyComponent;
  let fixture: ComponentFixture<NewKeyComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [NewKeyComponent]
    })
      .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(NewKeyComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
