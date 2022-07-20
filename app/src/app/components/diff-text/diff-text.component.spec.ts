/* tslint:disable:no-unused-variable */
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { DebugElement } from '@angular/core';

import { DiffTextComponent } from './diff-text.component';

describe('DiffTextComponent', () => {
  let component: DiffTextComponent;
  let fixture: ComponentFixture<DiffTextComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ DiffTextComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DiffTextComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
