'use client';

import React, { useContext, useEffect, useState } from 'react';
import { usePathname } from 'next/navigation';

import Collapse from '@mui/material/Collapse';
import ListItemButton from '@mui/material/ListItemButton';
import ListItemIcon from '@mui/material/ListItemIcon';
import ListItemText from '@mui/material/ListItemText';
import { styled, useTheme } from '@mui/material/styles';
import useMediaQuery from '@mui/material/useMediaQuery';
import { Theme } from '@mui/material/styles';

import NavItem from '../NavItem';
import { IconChevronDown, IconChevronUp } from '@tabler/icons-react';
import { useTranslation } from 'react-i18next';
import { CustomizerContext } from '@/app/context/customizerContext';

interface NavCollapseProps {
  menu: any;
  level: number;
  pathWithoutLastPart: string;
  pathDirect: string;
  hideMenu?: boolean;
  onClick: (event: React.MouseEvent<HTMLElement>) => void;
}

export default function NavCollapse({
  menu,
  level,
  pathWithoutLastPart,
  pathDirect,
  hideMenu,
  onClick,
}: NavCollapseProps) {
  const theme = useTheme();
  const pathname = usePathname();
  const lgDown = useMediaQuery((theme: Theme) =>
    theme.breakpoints.down('lg')
  );
  const { isBorderRadius } = useContext(CustomizerContext);
  const { t } = useTranslation();

  const Icon = menu.icon;
  const [open, setOpen] = useState(false);

  const isActive =
    pathDirect === menu.href ||
    pathDirect.startsWith(menu.href + '/');

  useEffect(() => {
    setOpen(false);
    menu.children?.forEach((item: any) => {
      if (pathname.startsWith(item.href)) {
        setOpen(true);
      }
    });
  }, [pathname, menu.children]);

  const ListItemStyled = styled(ListItemButton)(() => ({
    display: 'flex',
    alignItems: 'center',
    gap: 10,

    paddingTop: 8,
    paddingBottom: 8,

    // 🔑 HARUS SAMA DENGAN NavItem
    paddingLeft: hideMenu
      ? 12
      : level === 1
        ? 16
        : 16 + (level - 1) * 20,

    paddingRight: 16,
    marginBottom: 4,
    borderRadius: isBorderRadius,

    color: isActive
      ? theme.palette.primary.main
      : theme.palette.text.secondary,

    '&:hover': {
      backgroundColor: theme.palette.primary.light,
      color: theme.palette.primary.main,
    },

    '&.Mui-selected': {
      backgroundColor: theme.palette.primary.main,
      color: '#fff',
    },
  }));

  return (
    <>
      <ListItemStyled
        selected={isActive}
        onClick={() => setOpen(!open)}
      >
        <ListItemIcon
          sx={{
            minWidth: 'unset',
            width: 20,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            color: 'inherit',
          }}
        >
          <Icon size={18} stroke={1.5} />
        </ListItemIcon>

        {!hideMenu && (
          <ListItemText primary={t(menu.title)} />
        )}

        {!hideMenu &&
          (open ? (
            <IconChevronUp size={18} />
          ) : (
            <IconChevronDown size={18} />
          ))}
      </ListItemStyled>

      <Collapse in={open} timeout="auto">
        {menu.children?.map((item: any) =>
          item.children ? (
            <NavCollapse
              key={item.id}
              menu={item}
              level={level + 1}
              pathWithoutLastPart={pathWithoutLastPart}
              pathDirect={pathDirect}
              hideMenu={hideMenu}
              onClick={onClick}
            />
          ) : (
            <NavItem
              key={item.id}
              item={item}
              level={level + 1}
              pathDirect={pathDirect}
              hideMenu={hideMenu}
              onClick={lgDown ? onClick : () => {}}
            />
          )
        )}
      </Collapse>
    </>
  );
}
