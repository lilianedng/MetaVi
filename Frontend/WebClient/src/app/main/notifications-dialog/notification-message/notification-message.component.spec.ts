import {ComponentFixture, TestBed} from '@angular/core/testing';

import {NotificationMessageComponent} from './notification-message.component';

describe('NotificationMessageComponent', () => {
  let component: NotificationMessageComponent;
  let fixture: ComponentFixture<NotificationMessageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ NotificationMessageComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(NotificationMessageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
