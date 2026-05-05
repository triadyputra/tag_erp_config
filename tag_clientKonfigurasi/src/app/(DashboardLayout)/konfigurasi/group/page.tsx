"use client"; // <-- wajib

import React from "react";
import Breadcrumb from "@/app/(DashboardLayout)/layout/shared/breadcrumb/Breadcrumb";
import PageContainer from "@/app/components/container/PageContainer";
// import AkunList from "@/app/components/konfigurasi/akun/akun-list/index";
import BlankCard from "@/app/components/shared/BlankCard";
import { CardContent } from "@mui/material";
import GroupListComponent from "@/app/feature/konfigurasi/group";

const BCrumb = [
  {
    to: "/",
    title: "Home",
  },
  {
    title: "Daftar Group Akses",
  },
];

const AkunListing = () => {
  return (
        <PageContainer title="Daftar Group Akses" description="ini adalah daftar group akses aplikasi">
          <Breadcrumb title="Daftar Group Akses" items={BCrumb} />
          <BlankCard>
            <CardContent>
              <GroupListComponent/>
            </CardContent>
          </BlankCard>
        </PageContainer>
  );
}
export default AkunListing;
