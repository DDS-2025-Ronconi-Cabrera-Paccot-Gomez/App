import { Component, OnInit } from '@angular/core';
import { DynamicLayoutComponent } from '@abp/ng.core';
import { LoaderBarComponent } from '@abp/ng.theme.shared';
import { UserAvatarService } from './services/user-avatar.service';
import { UserAvatarDirective } from './directives/avatar.directive';
import { RoutesService, eLayoutType } from '@abp/ng.core';


@Component({
  selector: 'app-root',
  template: `
    <div appUserAvatar>
      <abp-loader-bar />
      <abp-dynamic-layout />
    </div>
  `,
  imports: [LoaderBarComponent, DynamicLayoutComponent, UserAvatarDirective],
})
export class AppComponent implements OnInit {
  constructor(private avatarService: UserAvatarService) {}

  ngOnInit() {
    this.avatarService.load();
  }
}
