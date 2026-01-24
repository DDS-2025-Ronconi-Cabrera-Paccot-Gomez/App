import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TopDestinations } from './top-destinations';

describe('TopDestinations', () => {
  let component: TopDestinations;
  let fixture: ComponentFixture<TopDestinations>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TopDestinations]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TopDestinations);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
